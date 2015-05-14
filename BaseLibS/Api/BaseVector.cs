﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace BaseLibS.Api{
	[Serializable]
	public abstract class BaseVector : ICloneable, IDisposable, IEnumerable<double>{
		/// <summary>
		/// Determines the scalar product of this vector with another one passed as the argument. 
		/// </summary>
		public abstract double Dot(BaseVector vec);

		/// <summary>
		/// Produces a deep copy of this vector.
		/// </summary>
		public abstract BaseVector Copy();

		/// <summary>
		/// Number of elements in this vector.
		/// </summary>
		public abstract int Length { get; }

		/// <summary>
		/// Indexer to the elements of this vector.
		/// </summary>
		public abstract double this[int index] { get; set; }

		/// <summary>
		/// Determines the sum of squared differences of this vector with another one passed as the argument. 
		/// </summary>
		public abstract double SumSquaredDiffs(BaseVector y1);

		/// <summary>
		/// Creates a new vector containing the elements that are indexed by the input array.
		/// </summary>
		public abstract BaseVector SubArray(IList<int> inds);

		/// <summary>
		/// True if at least one entry is NaN or Infinity.
		/// </summary>
		public abstract bool ContainsNaNOrInf();

		/// <summary>
		/// True if all entries are NaN or Infinity.
		/// </summary>
		public abstract bool IsNanOrInf();

		/// <summary>
		/// Performs tasks associated with freeing, releasing, or resetting resources.
		/// </summary>
		public abstract void Dispose();

		public abstract float[] Unpack();

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		public abstract IEnumerator<double> GetEnumerator();


		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator(){
			return GetEnumerator();
		}

		/// <summary>
		/// Produces a deep copy of this vector.
		/// </summary>
		public object Clone(){
			return Copy();
		}
	}
}