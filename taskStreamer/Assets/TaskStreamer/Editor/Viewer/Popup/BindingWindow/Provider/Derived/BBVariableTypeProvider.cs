using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
	public class BBVariableTypeProvider : ICategoryTreeProvider
	{
		static BBVariableTypeProvider()
		{
			foreach (Type[] types in _CategoryTypes.Values)
			{
				foreach (Type type in types)
				{
					_PredefinedTypes.Add(type);
				}
			}
		}


		private readonly static HashSet<Type> _PredefinedTypes = new HashSet<Type>();

		private readonly static Dictionary<string, Type[]> _CategoryTypes = new Dictionary<string, Type[]>
		{
			["Primitives"] = new[]
			{
				typeof(bool),
				typeof(int),
				typeof(float),
				typeof(double),
				typeof(string),
				typeof(long),
				typeof(byte),
				typeof(char)
			},
			["Math"] = new[]
			{
				typeof(Vector2),
				typeof(Vector3),
				typeof(Vector4),
				typeof(Vector2Int),
				typeof(Vector3Int),
				typeof(Quaternion),
				typeof(Color),
				typeof(Color32),
				typeof(Rect),
				typeof(RectInt),
				typeof(Bounds),
				typeof(BoundsInt),
				typeof(Matrix4x4),
				typeof(AnimationCurve),
				typeof(Gradient)
			},
			["Unity Object"] = new[]
			{
				typeof(GameObject),
				typeof(Transform),
				typeof(Component)
			},
			["Physics"] = new[]
			{
				typeof(Rigidbody),
				typeof(Rigidbody2D),
				typeof(Collider),
				typeof(Collider2D),
				typeof(CharacterController)
			},
			["Animation"] = new[]
			{
				typeof(Animator),
				typeof(AnimationClip),
				typeof(RuntimeAnimatorController)
			},
			["Navigation"] = new[]
			{
				typeof(NavMeshAgent)
			},
			["Audio"] = new[]
			{
				typeof(AudioSource),
				typeof(AudioClip)
			},
			["Rendering"] = new[]
			{
				typeof(ParticleSystem),
				typeof(Renderer),
				typeof(SpriteRenderer),
				typeof(MeshRenderer),
				typeof(Material),
				typeof(Mesh),
				typeof(Sprite),
				typeof(Texture),
				typeof(Texture2D)
			},
			["Assets"] = new[]
			{
				typeof(ScriptableObject)
			},
			["Misc"] = new[]
			{
				typeof(LayerMask),
				typeof(Enum)
			}
		};


		public SearchTreeEntry[] ProvideCategories(FactoryModule module)
		{
			List<SearchTreeEntry> entries = ListPool<SearchTreeEntry>.Get();

			// 헤더 엔트리 추가
			entries.Add(new SearchTreeGroupEntry(new GUIContent(module.title)) { level = module.layer });

			// 카테고리별로 그룹 엔트리와 항목 추가
			foreach (KeyValuePair<string, Type[]> category in _CategoryTypes)
			{
				string categoryName = category.Key;
				Type[] types = category.Value;

				if (types.Length == 0)
				{
					continue;
				}

				// 카테고리 그룹 엔트리
				entries.Add(new SearchTreeGroupEntry(new GUIContent(categoryName)) { level = module.layer + 1 });

				// 카테고리 내 타입들
				foreach (Type valueType in types)
				{
					string typeName = StringUtility.ToNicifyName(valueType.Name);
					SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(typeName))
					{
						userData = (valueType, module),
						level = module.layer + 2
					};

					entries.Add(entry);
				}
			}

			// 사용자 정의 타입 및 외부 라이브러리 타입 추가
			AddUserDefinedTypes(entries, module);

			SearchTreeEntry[] result = entries.ToArray();
			ListPool<SearchTreeEntry>.Release(entries);
			return result;
		}


		private void AddUserDefinedTypes(List<SearchTreeEntry> entries, FactoryModule module)
		{
			HashSet<Type> collectedTypes = HashSetPool<Type>.Get();

			// Serializable 속성이 있는 타입들 수집
			foreach (Type type in TypeCache.GetTypesWithAttribute<SerializableAttribute>())
			{
				if (IsValidType(type))
				{
					collectedTypes.Add(type);
				}
			}

			// UnityEngine.Object를 상속한 타입들 수집 (ScriptableObject, MonoBehaviour 등)
			foreach (Type type in TypeCache.GetTypesDerivedFrom<UnityEngine.Object>())
			{
				if (IsValidType(type))
				{
					collectedTypes.Add(type);
				}
			}

			if (collectedTypes.Count == 0)
			{
				HashSetPool<Type>.Release(collectedTypes);
				return;
			}

			// Other 카테고리 헤더
			entries.Add(new SearchTreeGroupEntry(new GUIContent("Other")) { level = module.layer + 1 });

			// 네임스페이스 루트로 그룹화
			IOrderedEnumerable<IGrouping<string, Type>> groupedByNamespace = collectedTypes.GroupBy(t => GetRootNamespace(t))
			                                                                               .OrderBy(g => g.Key);

			foreach (IGrouping<string, Type> group in groupedByNamespace)
			{
				// 라이브러리 서브 카테고리
				entries.Add(new SearchTreeGroupEntry(new GUIContent(group.Key)) { level = module.layer + 2 });

				// 해당 라이브러리의 타입들
				foreach (Type type in group.OrderBy(t => t.Name))
				{
					string typeName = StringUtility.ToNicifyName(type.Name);
					SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(typeName)) { userData = (type, module), level = module.layer + 3 };
					entries.Add(entry);
				}
			}

			HashSetPool<Type>.Release(collectedTypes);
		}


		private string GetRootNamespace(Type type)
		{
			string ns = type.Namespace;

			if (string.IsNullOrEmpty(ns))
			{
				return "Global";
			}

			int dotIndex = ns.IndexOf('.');

			if (dotIndex > 0)
			{
				return ns.Substring(0, dotIndex);
			}
			else
			{
				return ns;
			}
		}


		private bool IsValidType(Type type)
		{
			if (type == null)
			{
				return false;
			}

			if (type.IsAbstract)
			{
				return false;
			}

			if (type.IsGenericType)
			{
				return false;
			}

			if (type.IsInterface)
			{
				return false;
			}

			if (type.IsNested)
			{
				return false;
			}

			if (_PredefinedTypes.Contains(type))
			{
				return false;
			}

			if (IsEditorOnlyType(type))
			{
				return false;
			}

			return true;
		}


		private bool IsEditorOnlyType(Type type)
		{
			// 네임스페이스가 UnityEditor로 시작하는 경우
			if (type.Namespace != null && type.Namespace.StartsWith("UnityEditor"))
			{
				return true;
			}

			// 어셈블리 이름으로 에디터 전용 판별
			string assemblyName = type.Assembly.GetName().Name;

			if (assemblyName.StartsWith("UnityEditor"))
			{
				return true;
			}

			// 사용자 정의 에디터 어셈블리 (일반적인 명명 규칙)
			if (assemblyName.EndsWith(".Editor") || assemblyName.Contains(".Editor."))
			{
				return true;
			}

			return false;
		}
	}
}