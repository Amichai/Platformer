using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                if (Math.Abs(dVal) > 10) throw new Exception();
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
                int startIdx, endIndex;
                if (index1 < index2) {
                    startIdx = index1;
                    endIndex = index2;
                } else {
                    startIdx = index2;
                    endIndex = index1;
                }
                ///If these indices aren't the same, we passed at least one val
                for (int i = startIdx; i < endIndex; i++) {
                    toReturn.Add(orderedEdges[i]);
                }
                if (toReturn.Count() == 0) throw new Exception();
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
            var startEdgesInRange = startEdge.SpritesWithinRange(startVal, endVal);
            var endEdgesInRange = endEdge.SpritesWithinRange(startVal, endVal);
            toReturn = toReturn.AddRange(startEdgesInRange);
            toReturn = toReturn.AddRange(endEdgesInRange);

            ///We still haven't found the sprites that start before startVal and end after endVal
            var spritesStartingBeforeStartVal = startEdge.SpritesStartingBefore(startVal);
            var spritesEndingAfterEndVal = endEdge.SpritesEndingAfter(endVal);
            var overlap = spritesStartingBeforeStartVal.Overlap(spritesEndingAfterEndVal);
            toReturn = toReturn.AddRange(overlap);
            return toReturn;
        }

        private double collionIn1D(double s1a, double s1b, double s2a, double s2b) {
            Dictionary<string, double> horizEdges = new Dictionary<string, double>() {
                {"l1", s1a},
                {"r1", s1b},
                {"l2", s2a},
                {"r2", s2b},
            };
            var sorted = horizEdges.OrderBy(i => i.Value);
            if (sorted.ElementAt(0).Key.Last() == sorted.ElementAt(1).Key.Last()) {
                return 0;
            } else {
                return sorted.ElementAt(2).Value - sorted.ElementAt(1).Value;
            }
        }

        private GameInstance.CollisionType collisionType(int sprite1, int sprite2) {
            var s1Top = y0Edges.GetVal(sprite1);
            var s1Bottom = y1Edges.GetVal(sprite1);
            var s1Left = x0Edges.GetVal(sprite1);
            var s1Right = x1Edges.GetVal(sprite1);
            var s2Top = y0Edges.GetVal(sprite2);
            var s2Bottom = y1Edges.GetVal(sprite2);
            var s2Left = x0Edges.GetVal(sprite2);
            var s2Right = x1Edges.GetVal(sprite2);
            ///Sort four edges and test for arrangement
            var horizCollision = collionIn1D(s1Left, s1Right, s2Left, s2Right);
            var vertCollision = collionIn1D(s1Top, s1Bottom, s2Top, s2Bottom);

            if (horizCollision == 0 || vertCollision == 0) {
                return GameInstance.CollisionType.none;
            }

            if (vertCollision < horizCollision) {
                //vert collision
                if (s1Top < s2Top) {
                    return GameInstance.CollisionType.bottom;
                } else {
                    return GameInstance.CollisionType.top;
                }

            }
            if (horizCollision < vertCollision) {
                //horiz collision
                if (s1Left < s2Left) {
                    return GameInstance.CollisionType.right;
                } else {
                    return GameInstance.CollisionType.left;
                }
            }
            throw new Exception();
            
        }

        private void addYcollisions(int i, List<int> vals) {
            vals.Remove(i);
            if (vals.Count() == 0) return;
            spriteCollisionsY[i] = spriteCollisionsY[i].AddRange(vals);
        }

        private void removeYcollisions(int i, List<int> vals) {
            vals.Remove(i);
            if (vals.Count() == 0) return;
            spriteCollisionsY[i] = spriteCollisionsY[i].RemoveAll(vals);
        }

        private void addXcollisions(int i, List<int> vals) {
            vals.Remove(i);
            if (vals.Count() == 0) return;
            spriteCollisionsX[i] = spriteCollisionsX[i].AddRange(vals);
        }

        private void removeXcollisions(int i, List<int> vals) {
            vals.Remove(i);
            if (vals.Count() == 0) return;
            spriteCollisionsX[i] = spriteCollisionsX[i].RemoveAll(vals);
        }

        public GameInstance.CollisionType Update_Slow(int i, double x0, double y0, double x1, double y1) {
            spriteCollisionsX[i] = getXIntersectingSprites(x0, x1);
            spriteCollisionsX[i].Remove(i);
            spriteCollisionsY[i] = getYIntersectingSprites(y0, y1);
            spriteCollisionsY[i].Remove(i);
            if (!knownIndices.Contains(i)) {
                knownIndices.Add(i);
                x0Edges.Add(i, x0);
                y0Edges.Add(i, y0);
                x1Edges.Add(i, x1);
                y1Edges.Add(i, y1);
            }

            x0Edges.Update(i, x0);
            x1Edges.Update(i, x1);
            y0Edges.Update(i, y0);
            y1Edges.Update(i, y1);

            var union = spriteCollisionsX[i].Overlap(spriteCollisionsY[i]);
            if (union.Count() > 0) {
                return collisionType(i, union.First());
            }
            return GameInstance.CollisionType.none;
        }

        public GameInstance.CollisionType Update(int i, double x0, double y0, double x1, double y1) {
            if (!knownIndices.Contains(i)) {
                spriteCollisionsX[i] = getXIntersectingSprites(x0, x1);
                spriteCollisionsX[i].Remove(i);
                spriteCollisionsY[i] = getYIntersectingSprites(y0, y1);
                spriteCollisionsY[i].Remove(i);
                knownIndices.Add(i);
                x0Edges.Add(i, x0);
                y0Edges.Add(i, y0);
                x1Edges.Add(i, x1);
                y1Edges.Add(i, y1);
            }

            var dx = x0Edges.GetVal(i) - x0;
            var dy = y0Edges.GetVal(i) - y0;
            
            List<int> toAdd, toRemove;
            if (dx < 0) {
                ///lost collisions in x
                toAdd = x1Edges.SpritesWithinRange(x0Edges.GetVal(i), x0);
                addXcollisions(i, toAdd);

                toRemove = x0Edges.SpritesWithinRange(x1Edges.GetVal(i), x1);
                removeXcollisions(i, toRemove);
            } else {
                toAdd = x0Edges.SpritesWithinRange(x1Edges.GetVal(i), x1);
                addXcollisions(i, toAdd);

                toRemove = x1Edges.SpritesWithinRange(x0Edges.GetVal(i), x0);
                removeXcollisions(i, toRemove);
            }

            x0Edges.Update(i, x0);
            x1Edges.Update(i, x1);

            if (dy < 0) {
                toRemove = y1Edges.SpritesWithinRange(y0Edges.GetVal(i), y0);
                removeYcollisions(i, toRemove);

                toAdd = y0Edges.SpritesWithinRange(y1Edges.GetVal(i), y1);
                addYcollisions(i, toAdd);
            } else {
                toRemove = y0Edges.SpritesWithinRange(y1Edges.GetVal(i), y1);
                removeYcollisions(i, toRemove);

                toAdd = y1Edges.SpritesWithinRange(y0Edges.GetVal(i), y0);
                addYcollisions(i, toAdd);
            }
            
            y0Edges.Update(i, y0);
            y1Edges.Update(i, y1);

            //Debug.Print("X coll: " + spriteCollisionsX[i].Count().ToString());
            //Debug.Print("Y coll: " + spriteCollisionsY[i].Count().ToString());
            var union = spriteCollisionsX[i].Overlap(spriteCollisionsY[i]);
            if (union.Count() > 0) {
                if (union.Count() > 1) {
                    throw new Exception();
                }
                return collisionType(i, union.First());
            }
            return GameInstance.CollisionType.none;
        }
    }
}
