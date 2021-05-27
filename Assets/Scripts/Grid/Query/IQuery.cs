using System.Collections.Generic;

#if SPATIAL_GRID
namespace Spatial
{
    public interface IQuery
    {
        IEnumerable<IGridEntity> Query();
    }
}
#endif