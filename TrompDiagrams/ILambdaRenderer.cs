using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public interface ILambdaRenderer
{
	public Geometry Render(Dictionary<string, int> variableHeights, int currentHeight = 0);
}