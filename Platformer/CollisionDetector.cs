using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer {
    public class CollisionDetector {
        public class spriteEdge {
            Dictionary<int, double> edgePosition = new Dictionary<int, double>();
            Dictionary<int, int> edgeRank = new Dictionary<int, int>();
            List<int> orderedEdges = new List<int>();
            int[,] overlap;

            private void swapEdgeRank(int idx1, int idx2) {
                var c = edgeRank[idx1];
                edgeRank[idx1] = edgeRank[idx2];
                edgeRank[idx2] = c;
            }

            /// <summary>
            /// Check for new overlaps
            /// </summary>
            private void swapEdgeOrder(int rank1, int rank2) {
                var c = orderedEdges[rank1];
                orderedEdges[rank1] = orderedEdges[rank2];
                orderedEdges[rank2] = c;
            }

            public void Update(int i, double val) {
                var dVal = val - edgePosition[i];
                edgePosition[i] = val;
                ///Check who we surpassed
                ///This is the old rank of the recently moved edge
                var rank = edgeRank[i];

                int offset = 0;
                int idx1, idx2;
                do {
                    var rank1 = rank + offset;
                    int rank2;
                    idx1 = orderedEdges[rank1];
                    if (dVal > 0) {
                        rank2 = ++offset + rank;
                        idx2 = orderedEdges[rank2];
                        if (edgePosition[idx1] > edgePosition[idx2]) {
                            swapEdgeRank(idx1, idx2);
                            swapEdgeOrder(rank1, rank2);
                            ///these two are out of order and should be swapped
                        } else break;
                    } else{
                        rank2 = --offset + rank;
                        idx2 = orderedEdges[rank2];
                        if (edgePosition[idx1] < edgePosition[idx2]) {
                            swapEdgeRank(idx1, idx2);
                            swapEdgeOrder(rank1, rank2);
                            ///these two are out of order and should be swapped
                        } else break;
                    }
                } while (true);
                ///We might have to increase this rank!
            }
        }

        spriteEdge x0Edges = new spriteEdge();
        spriteEdge y0Edges = new spriteEdge();
        spriteEdge x1Edges = new spriteEdge();
        spriteEdge y1Edges = new spriteEdge();

        Dictionary<int, double> spriteX0 = new Dictionary<int, double>();
        Dictionary<int, double> spriteX1 = new Dictionary<int, double>();
        Dictionary<int, double> spriteY0 = new Dictionary<int, double>();
        Dictionary<int, double> spriteY1 = new Dictionary<int, double>();

        Dictionary<int, int> idxRankX0 = new Dictionary<int, int>();
        Dictionary<int, int> idxRankX1 = new Dictionary<int, int>();
        Dictionary<int, int> idxRankY0 = new Dictionary<int, int>();
        Dictionary<int, int> idxRankY1 = new Dictionary<int, int>();

        List<int> orderedByX0 = new List<int>();
        List<int> orderedByX1 = new List<int>();
        List<int> orderedByY0 = new List<int>();
        List<int> orderedByY1 = new List<int>();

        bool[][] collisionsInX = new bool[0][];
        bool[][] collisionsInY = new bool[0][];

        public void Update(int i, double x0, double y0, double x1, double y1) {
            x0Edges.Update(i, x0);
            ///We are assuming the bodies can't deform after creation
            
            spriteX1[i] = x1;
            var dy = y0 - spriteY0[i];
            spriteY0[i] = y0;
            spriteY1[i] = y1;
        }
    }
}
