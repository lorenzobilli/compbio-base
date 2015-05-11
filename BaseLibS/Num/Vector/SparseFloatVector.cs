﻿using System;
using System.Collections.Generic;
using BaseLibS.Api;
using BaseLibS.Util;

namespace BaseLibS.Num.Vector{
	[Serializable]
	public class SparseFloatVector : BaseVector{
		/// <summary>
		/// Indices of nonzero elements. Indices are sorted.
		/// </summary>
		internal int[] indices;

		/// <summary>
		/// Values at the positions specified by the <code>indices</code> array.
		/// </summary>
		internal float[] values;

		/// <summary>
		/// Total length of the vector.
		/// </summary>
		private readonly int length;

		public SparseFloatVector(IList<float> values){
			List<int> newIndices = new List<int>();
			List<float> newValues = new List<float>();
			for (int i = 0; i < values.Count; i++){
				if (values[i] != 0){
					newValues.Add(values[i]);
					newIndices.Add(i);
				}
			}
			indices = newIndices.ToArray();
			this.values = newValues.ToArray();
			length = values.Count;
		}

		public SparseFloatVector(int[] indices, float[] values, int length){
			this.indices = indices;
			this.values = values;
			this.length = length;
		}

		public override int Length{
			get { return length; }
		}

		public override BaseVector Copy(){
			int[] newIndices = new int[indices.Length];
			Array.Copy(indices, newIndices, indices.Length);
			float[] newValues = new float[values.Length];
			Array.Copy(values, newValues, values.Length);
			return new SparseFloatVector(newIndices, newValues, length);
		}

		public override BaseVector SubArray(IList<int> inds){
			List<int> newIndices = new List<int>();
			List<float> newValues = new List<float>();
			for (int i = 0; i < inds.Count; i++){
				int x = Array.BinarySearch(indices, inds[i]);
				if (x >= 0){
					newIndices.Add(i);
					newValues.Add(values[x]);
				}
			}
			return new SparseFloatVector(newIndices.ToArray(), newValues.ToArray(), inds.Count);
		}

		public override IEnumerator<double> GetEnumerator(){
			throw new NotImplementedException();
		}

		public override bool ContainsNaNOrInfinity(){
			foreach (float value in values){
				if (float.IsNaN(value) || float.IsInfinity(value)){
					return true;
				}
			}
			return false;
		}

		public override void Dispose(){
			indices = null;
			values = null;
		}

		public override double this[int i]{
			get{
				int ind = Array.BinarySearch(indices, i);
				return ind < 0 ? 0 : values[ind];
			}
			set{
				int ind = Array.BinarySearch(indices, i);
				if (ind >= 0){
					values[ind] = (float) value;
				} else{
					if (value == 0){
						return;
					}
					int insertPos = -1 - ind;
					values = ArrayUtils.Insert(values, (float) value, insertPos);
					indices = ArrayUtils.Insert(indices, i, insertPos);
				}
			}
		}

		public override double Dot(BaseVector y){
			if (y is SparseBoolVector){
				return SparseBoolVector.Dot((SparseBoolVector) y, this);
			}
			if (y is FloatArrayVector){
				return Dot((FloatArrayVector) y, this);
			}
			if (y is DoubleArrayVector){
				return Dot((DoubleArrayVector) y, this);
			}
			if (y is BoolArrayVector){
				return Dot((BoolArrayVector) y, this);
			}
			return Dot(this, (SparseFloatVector) y);
		}

		public override double SumSquaredDiffs(BaseVector y){
			if (y is FloatArrayVector){
				return SumSquaredDiffs((FloatArrayVector) y, this);
			}
			if (y is DoubleArrayVector){
				return SumSquaredDiffs((DoubleArrayVector) y, this);
			}
			if (y is BoolArrayVector){
				return SumSquaredDiffs((BoolArrayVector) y, this);
			}
			return SumSquaredDiffs(this, (SparseFloatVector) y);
		}

		internal static double Dot(SparseFloatVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.indices.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (x.indices[i] == y.indices[j]){
					sum += x.values[i++]*y.values[j++];
				} else{
					if (x.indices[i] > y.indices[j]){
						++j;
					} else{
						++i;
					}
				}
			}
			return sum;
		}

		internal static double Dot(FloatArrayVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (i == y.indices[j]){
					sum += x.values[i++]*y.values[j++];
				} else{
					if (i > y.indices[j]){
						++j;
					} else{
						++i;
					}
				}
			}
			return sum;
		}

		internal static double Dot(BoolArrayVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (i == y.indices[j]){
					if (x.values[i]){
						sum += y.values[j];
					}
					i++;
					j++;
				} else{
					if (i > y.indices[j]){
						++j;
					} else{
						++i;
					}
				}
			}
			return sum;
		}

		internal static double Dot(DoubleArrayVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (i == y.indices[j]){
					sum += x.values[i++]*y.values[j++];
				} else{
					if (i > y.indices[j]){
						++j;
					} else{
						++i;
					}
				}
			}
			return sum;
		}

		internal static double SumSquaredDiffs(SparseFloatVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.indices.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (x.indices[i] == y.indices[j]){
					double d = x.values[i++] - y.values[j++];
					sum += d*d;
				} else if (x.indices[i] > y.indices[j]){
					sum += y.values[j]*y.values[j];
					++j;
				} else{
					sum += x.values[i]*x.values[i];
					++i;
				}
			}
			while (i < xlen){
				sum += x.values[i]*x.values[i];
				++i;
			}
			while (j < ylen){
				sum += y.values[j]*y.values[j];
				++j;
			}
			return sum;
		}

		internal static double SumSquaredDiffs(FloatArrayVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (i == y.indices[j]){
					double d = x.values[i++] - y.values[j++];
					sum += d*d;
				} else if (i > y.indices[j]){
					sum += y.values[j]*y.values[j];
					++j;
				} else{
					sum += x.values[i]*x.values[i];
					++i;
				}
			}
			while (i < xlen){
				sum += x.values[i]*x.values[i];
				++i;
			}
			while (j < ylen){
				sum += y.values[j]*y.values[j];
				++j;
			}
			return sum;
		}

		internal static double SumSquaredDiffs(DoubleArrayVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (i == y.indices[j]){
					double d = x.values[i++] - y.values[j++];
					sum += d*d;
				} else if (i > y.indices[j]){
					sum += y.values[j]*y.values[j];
					++j;
				} else{
					sum += x.values[i]*x.values[i];
					++i;
				}
			}
			while (i < xlen){
				sum += x.values[i]*x.values[i];
				++i;
			}
			while (j < ylen){
				sum += y.values[j]*y.values[j];
				++j;
			}
			return sum;
		}

		internal static double SumSquaredDiffs(BoolArrayVector x, SparseFloatVector y){
			double sum = 0;
			int xlen = x.Length;
			int ylen = y.indices.Length;
			int i = 0;
			int j = 0;
			while (i < xlen && j < ylen){
				if (i == y.indices[j]){
					double d = y.values[j++];
					if (x.values[i++]){
						d -= 1;
					}
					sum += d*d;
				} else if (i > y.indices[j]){
					sum += y.values[j]*y.values[j];
					++j;
				} else{
					if (x.values[i]){
						sum += 1;
					}
					++i;
				}
			}
			while (i < xlen){
				if (x.values[i]){
					sum += 1;
				}
				++i;
			}
			while (j < ylen){
				sum += y.values[j]*y.values[j];
				++j;
			}
			return sum;
		}

		public override bool IsNanOrInf(){
			if (values.Length < length){
				return false;
			}
			foreach (float value in values){
				if (!float.IsNaN(value) && !float.IsInfinity(value)){
					return false;
				}
			}
			return true;
		}

		public override float[] Unpack(){
			float[] result = new float[length];
			for (int i = 0; i < values.Length; i++){
				result[indices[i]] = values[i];
			}
			return result;
		}
	}
}