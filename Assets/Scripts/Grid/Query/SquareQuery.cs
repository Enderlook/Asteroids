using System.Collections.Generic;
using UnityEngine;

#if SPATIAL_GRID
namespace Spatial
{
    public class SquareQuery : MonoBehaviour, IQuery
    {
        public SpatialGrid targetGrid;
        public float width = 15f;
        public float height = 30f;
        public IEnumerable<IGridEntity> selected = new List<IGridEntity>();

        public IEnumerable<IGridEntity> Query()
        {
            var h = height * 0.5f;
            var w = width * 0.5f;
            //posicion inicial --> esquina superior izquierda de la "caja"
            //posición final --> esquina inferior derecha de la "caja"
            //como funcion para filtrar le damos una que siempre devuelve true, para que no filtre nada.
            return targetGrid.Query(
                                    (Vector2)transform.position + new Vector2(-w,-h),
                                    (Vector2)transform.position + new Vector2(w, h),
                                    x => true);
        }

        void OnDrawGizmos()
        {
            if (targetGrid == null) return;

            //Flatten the sphere we're going to draw
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector2(width, height));
        }
    }
}
#endif