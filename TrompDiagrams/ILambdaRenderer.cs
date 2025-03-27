using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public interface ILambdaRenderer
{
	public Geometry Render(Dictionary<Guid, int> variableHeights, int currentHeight=0);
}