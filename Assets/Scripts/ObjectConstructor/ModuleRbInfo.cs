public abstract class RigidbodyInfo
{
	abstract public float mass {get;}
	abstract public float drag {get;}
	abstract public float angularDrag {get;}
	public float gravityScale  {get{return 0;}}
}

public class PilotModuleRigidbodyInfo : RigidbodyInfo
{
	override public float mass {get{return 1;}}
	override public float drag {get{return 4;}}
	override public float angularDrag {get{return 10;}}
}

public class RandomModuleRigidbodyInfo : RigidbodyInfo
{
	override public float mass {get{return 0.5f;}}
	override public float drag {get{return 2;}}
	override public float angularDrag {get{return 4;}}
}
