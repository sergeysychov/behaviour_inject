using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourInject;

public class HierarchyContextImpl : HierarchyContext {

	private Context _local;

	public override Context GetContext()
	{
		if(_local == null)
		{
			_local = Context.CreateLocal()
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(new DataModel("local data"));
		}
		return _local;
	}
}
