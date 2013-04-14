using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer {
    public class CollisionDetector {
        spriteEdge x0Edges { get; set; }
        spriteEdge y0Edges { get; set; }
        spriteEdge x1Edges { get; set; }
        spriteEdge y1Edges { get; set; }
        public CollisionDetector() {
            this.x0Edges = new spriteEdge();
            this.y0Edges = new spriteEdge();
            this.x1Edges = new spriteEdge();
            this.y1Edges = new spriteEdge();
        }

        Dictionary<int, HashSet<int>> spriteCollisionsX = new Dictionary<int, HashSet<int>>();
        Dictionary<int, HashSet<int>> spriteCollisionsY = new Dictionary<int, HashSet<int>>();

        private class spriteEdge {
            Dictionary<int, double> edgePosition = new Dictionary<int, double>();
            Dictionary<int, int> edgeRank = new Dictionary<int, int>();
            List<int> orderedEdges = new List<int>();

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

            public void Update(int i, double newVal) {
                double oldVal = edgePosition[i];
                var dVal = newVal - oldVal;
                if (dVal == 0) return;
                edgePosition[i] = newVal;
                ///Check who we surpassed
                ///This is the old rank of the recently moved edge
                var oldRank = edgeRank[i];
                ///Test for rank change
                int offset = 0;
                int idx1, idx2;
                do {
                    int rank1 = oldRank + offset;
                    idx1 = orderedEdges[rank1]; //This is the sprite index at this rank
                    int rank2;
                    if (dVal > 0) {
                        rank2 = ++offset + oldRank;
                        if (rank2 == orderedEdges.Count()) break;
                        idx2 = orderedEdges[rank2]; //adjacent sprite index
                        if (edgePosition[idx1] > edgePosition[idx2]) {
                            swapEdgeRank(idx1, idx2);
                            swapEdgeOrder(rank1, rank2);
                            ///these two are out of order and should be swapped
                        } else break;
                    } else{
                        rank2 = --offset + oldRank;
                        if (rank2 < 0) break;
                        idx2 = orderedEdges[rank2];
                        if (edgePosition[idx1] < edgePosition[idx2]) {
                            swapEdgeRank(idx1, idx2);
                            swapEdgeOrder(rank1, rank2);
                            ///these two are out of order and should be swapped
                        } else break;
                    }
                } while (true);
            }

            private int mid(int low, int high) {
                return (int)Math.Floor((low + high) / 2.0);
            }

            private int insertionIndex(double val) {
                int lowerBound = 0;
                int upperBound = orderedEdges.Count();
                int midIdx = 0;
                while (true) {
                    midIdx = mid(lowerBound, upperBound);
                    if (midIdx == orderedEdges.Count()) {
                        break;
                    }
                    int inspectionSprite = orderedEdges[midIdx];
                    double inspectionPosition = edgePosition[inspectionSprite];
                    if (val < inspectionPosition) {
                        if (upperBound == midIdx) break;
                        upperBound = midIdx;
                    } else {
                        if (lowerBound == midIdx) {
                            midIdx++;
                            break;
                        }
                        lowerBound = midIdx;
                    }
                }
                return midIdx;
            }

            public void Add(int i, double val) {
                this.edgePosition[i] = val;
                //edge rank
                //ordered edge
                //binary search over ordered edges, find new rank, update all old ranks
                var newIndex = insertionIndex(val);
                orderedEdges.Insert(newIndex, i);
                for (int j = newIndex; j < orderedEdges.Count(); j++) {
                    edgeRank[orderedEdges[j]] = j;
                }
            }

            internal List<int> SpritesWithinRange(double p1, double p2) {
                List<int> toReturn = new List<int>();
                if (p1 == p2) return toReturn;
                var index1 = insertionIndex(p1);
                var index2 = insertionIndex(p2);

                if (index1 == index2) return toReturn;
                ///If these indices aren't the same, we passed at least one val
                for (int i = index1; i < index2; i++) {
                    toReturn.Add(orderedEdges[i]);
                }
                return toReturn;
            }

            internal double GetVal(int i) {
                return edgePosition[i];
            }

            internal IEnumerable<int> SpritesStartingBefore(double x) {
                return edgePosition.Where(i => i.Value < x).Select(i => i.Key);
            }

            internal IEnumerable<int> SpritesEndingAfter(double x) {
                return edgePosition.Where(i => i.Value > x).Select(i => i.Key);
            }
        }

        HashSet<int> knownIndices = new HashSet<int>();

        Dictionary<int, List<int>> xCollisions = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> yCollisions = new Dictionary<int, List<int>>();

        /// <summary>This is a linear time operation</summary>
        private HashSet<int> getXIntersectingSprites(double x0, double x1) {
            return getIntersectingSprites(x0Edges, x1Edges, x0, x1);
        }

        private HashSet<int> getYIntersectingSprites(double y0, double y1) {
            return getIntersectingSprites(y0Edges, y1Edges, y0, y1);
        }

        private HashSet<int> getIntersectingSprites(spriteEdge startEdge, spriteEdge endEdge, double startVal, double endVal) {
            HashSet<int> toReturn = new HashSet<int>();
            toReturn = toReturn.AddRange(startEdge.SpritesWithinRange(startVal, endVal));
            toReturn = toReturn.MyUnion(endEdge.SpritesWithinRange(startVal, endVal));

            ///We still haven't found the sprites that start before startVal and end after endVal
            var spritesStartingBeforeStartVal = startEdge.SpritesStartingBefore(startVal);
            var spritesEndingAfterEndVal = endEdge.SpritesEndingAfter(endVal);
            var overlap = spritesStartingBeforeStartVal.Overlap(spritesEndingAfterEndVal);
            toReturn = toReturn.AddRange(overlap);
            return toReturn;
        }

        public void Update(int i, double x0, double y0, double x1, double y1) {
            if (!knownIndices.Contains(i)) {
                spriteCollisionsX[i] = getXIntersectingSprites(x0, x1);
                spriteCollisionsY[i] = getYIntersectingSprites(y0, y1);
                knownIndices.Add(i);
                x0Edges.Add(i, x0);
                y0Edges.Add(i, y0);
                x1Edges.Add(i, x1);
                y1Edges.Add(i, y1);
            }

            ///Update x0 position

            ///lost collisions in x
            var expiredXCollisions = x1Edges.SpritesWithinRange(x0Edges.GetVal(i), x0);
            spriteCollisionsX[i].RemoveAll(expiredXCollisions);
            x0Edges.Update(i, x0);

            ///Lost collisions in y
            var expiredYCollisions = y1Edges.SpritesWithinRange(y0Edges.GetVal(i), y0);
            spriteCollisionsY[i].RemoveAll(expiredYCollisions);
            y0Edges.Update(i, y0);

            ///See if this x1 passes an x0 - check for new collisions
            var newXCollisions = x0Edges.SpritesWithinRange(x1Edges.GetVal(i), x1);
            spriteCollisionsX[i].AddRange(newXCollisions);
            x1Edges.Update(i, x1);

            ///Check for collision
            var newYCollisions = y0Edges.SpritesWithinRange(y1Edges.GetVal(i), y1);
            spriteCollisionsY[i].AddRange(newYCollisions);
            y1Edges.Update(i, y1);

            var union = spriteCollisionsX[i].Overlap(spriteCollisionsY[i]);
            if (union.Count() > 0) {

            }
        }
    }
}
