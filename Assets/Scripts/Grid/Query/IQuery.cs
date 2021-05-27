using System.Collections.Generic;

namespace Spatial
{
    public interface IQuery
    {
        IEnumerable<IGridEntity> Query();
    }
}