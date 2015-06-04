public abstract class ModuleRbInfo
{
	public float mass;
	public float drag;
	public float angularDrag;
}

public class PilotModuleRbInfo : ModuleRbInfo
{
	public float mass = 1;
	public float drag = 4;
	public float angularDrag = 10;
}
