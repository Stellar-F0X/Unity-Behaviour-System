using System;
using System.Collections.Generic;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	/// <summary>
	/// BehaviorNodeView 클래스는 그래프 뷰에서 행위 노드(Behavior Node)를 시각화하고 관리하는 역할을 수행한다.
	/// </summary>
	/// <remarks>
	/// 이 클래스는 NodeViewBase를 상속받아 동작 노드에 필요한 포트 생성, 자식 정렬 등 고유한 기능을 제공한다.
	/// </remarks>
	internal class BehaviorNodeView : NodeViewBase
	{
		/// <summary>
		/// BehaviorNodeView는 특정 노드의 UI 뷰를 정의하며, 노드의 표현과 동작을 관리한다.
		/// </summary>
		private BehaviorNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml)
		{
			string nodeName = StringUtility.ToNicifyName(targetNode.name, "Node");
			base._nodeTypeLabel.text = StringUtility.ToNicifyName(targetNode.GetType().Name, "Node");
			base.targetNode.name = nodeName;
			base.title = nodeName;

			this._serviceContainerView = this.Q<VisualElement>("service-container");
			this._elementGroup.AddToClassList($"behavior-node");

			this.Indicator = new BehaviorIndicator(this, TSEditor.settings);

			List<ServiceBase> services = ((BehaviorNodeBase)targetNode).services;
			this._observableServiceList = new ObservableList<ServiceBase>(services);
			services.ForEach(s => this._serviceContainerView.Add(new ServiceBlock(s)));
			this._observableServiceList.onChanged += this.OnObservableServiceViewListChanged;
		}


		/// <summary>
		/// 서비스 리스트의 변경 이벤트를 처리하는 델리게이트입니다.
		/// </summary>
		/// <param name="changeType">리스트 변경 유형을 나타내는 값입니다.</param>
		/// <param name="service">변경된 서비스 인스턴스입니다.</param>
		private readonly ObservableList<ServiceBase> _observableServiceList;


		/// <summary>
		/// 서비스 뷰를 포함하는 컨테이너 역할을 하는 VisualElement.
		/// </summary>
		/// <remarks>
		/// 특정 노드의 서비스 리스트를 시각적으로 관리하고 표시합니다.
		/// </remarks>
		private readonly VisualElement _serviceContainerView;


		/// <summary>
		/// BehaviorNodeBase의 services 리스트를 랩핑하여 제공하는 읽기 전용 속성.
		/// </summary>
		/// <remarks>
		/// BehaviorNodeBase에서 관리되는 서비스 리스트를 반환하며, 이를 UI 요소나 기능에 활용할 수 있도록 한다.
		/// </remarks>
		public ObservableList<ServiceBase> observableServiceList
		{
			get { return _observableServiceList; }
		}



		//TODO: Unity가 C# 11을 지원하면 Static Abstract Interface로 StateNodeView와 함께 팩토리 함수를 묶자.
		/// <summary>
		/// BehaviorNodeView 인스턴스를 생성하여 초기화하고 반환한다.
		/// </summary>
		/// <param name="node">BehaviorNodeView의 대상이 되는 NodeBase 객체.</param>
		/// <param name="nodeXml">BehaviorNodeView의 UI 구성을 위한 VisualTreeAsset.</param>
		/// <returns>초기화된 BehaviorNodeView 인스턴스.</returns>
		/// <exception cref="System.Diagnostics.Debug.AssertFailedException">생성된 nodeView가 null인 경우 발생.</exception>
		public static BehaviorNodeView Create(NodeBase node, VisualTreeAsset nodeXml)
		{
			BehaviorNodeView nodeView = new BehaviorNodeView(node, nodeXml);
			Debug.Assert(nodeView is not null, "nodeView is null");

			nodeView.OnInitialize();
			nodeView.CreatePorts();
			return nodeView;
		}



		/// <summary> 서비스 목록의 변경 사항에 따라 서비스 뷰를 업데이트한다. </summary>
		/// <param name="action"> 서비스 추가 또는 삭제 상태를 나타내는 값. </param>
		/// <param name="service"> 변경된 서비스의 인스턴스. </param>
		/// <exception cref="System.ArgumentNullException"> null 참조된 파라미터가 존재할 경우 발생. </exception>
		/// <exception cref="System.ArgumentException"> serviceList에 존재하지 않는 서비스가 Remove 시도될 경우 발생. </exception>
		private void OnObservableServiceViewListChanged(NotifyListChanged action, ServiceBase service, int index)
		{
			Assert.IsNotNull(this._serviceContainerView, "Service container view is not found");
			Assert.IsNotNull(this.observableServiceList, "Service list is null");
			Assert.IsNotNull(service, "Service is null");

			switch (action)
			{
				case NotifyListChanged.Add: this._serviceContainerView.Add(new ServiceBlock(service)); break;

				case NotifyListChanged.Remove: this._serviceContainerView.RemoveAt(index); break;
			}

			Assert.IsNotNull(TSEditor.Instance.graphAsset, "graph asset is not found");
			UnityEditor.EditorUtility.SetDirty(TSEditor.Instance.graphAsset);
		}



		/// <summary>
		/// 노드가 그래프뷰에서 개발자에 의해 위치가 변경될 때, x 축 기준으로 자식들을 정렬한다.
		/// </summary>
		/// <exception cref="InvalidCastException">
		/// 대상 노드가 <see cref="BehaviorNodeBase"/> 타입이 아니거나 <see cref="CompositeNode"/>로 캐스팅할 수 없는 경우 발생한다.
		/// </exception>
		public void SortChildren()
		{
			if (((BehaviorNodeBase)targetNode).nodeType != BehaviorNodeType.Composite)
			{
				return;
			}

			if (targetNode is CompositeNode compositeNode)
			{
				compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
			}
		}



		/// <summary>
		/// 새로운 포트를 생성하고 초기화하여 반환한다.
		/// </summary>
		/// <param name="orientation">포트의 배치 방향을 지정한다.</param>
		/// <param name="direction">포트의 입력/출력 방향을 지정한다.</param>
		/// <param name="capacity">포트가 지원하는 연결 용량을 지정한다.</param>
		/// <param name="type">포트에서 처리할 데이터 유형을 지정한다.</param>
		/// <returns>생성된 <see cref="Port"/> 객체를 반환한다.</returns>
		public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
		{
			return new PortView(GraphType.BT, direction, capacity);
		}



		/// <summary>
		/// 노드의 유형에 따라 적절한 입력 및 출력 포트를 생성하고, 포트를 컨테이너에 배치한다.
		/// </summary>
		/// <remarks>
		/// 포트는 BehaviorNodeType에 따라 생성되며, 입력 포트와 출력 포트의 방향 및 용량이 달라질 수 있다.
		/// </remarks>
		protected override void CreatePorts()
		{
			switch (((BehaviorNodeBase)targetNode).nodeType)
			{
				case BehaviorNodeType.Root:
				{
					outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
					break;
				}

				case BehaviorNodeType.Action:
				{
					inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
					break;
				}

				case BehaviorNodeType.SubGraph:
				{
					inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
					break;
				}

				case BehaviorNodeType.Composite:
				{
					inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
					outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
					break;
				}

				case BehaviorNodeType.Decorator:
				{
					inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
					outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
					break;
				}
			}

			this.SetPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
			this.SetPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
		}
	}
}