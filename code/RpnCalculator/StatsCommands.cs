/*
 * StatsCommands.cs
 * 
 * Copyright (c) 2007 Bret Mulvey. All Rights Reserved.
 * Contact bretmulvey@hotmail.com for permission to redistribute.
 * 
 * This source file is part of the RPN Calculator application,
 * original published at http://bretm.home.comcast.net
 * 
 * This file implements statistics-related functions, including
 * basic mean and standard-deviation calculations, generation of
 * uniformly- and normally-distributed random numbers, and calculation
 * of the cumulative normal distribution and its inverse.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Mulvey.QuadPrecision;

namespace Mulvey.RpnCalculator
{
	[CommandContainer]
	public class StatsCommands
	{
		CalculatorEngine engine;

		Number mean;
		Number stdev;
		int statCount;
		Random rand = new Random();
		byte[] bytes = new byte[8];

		public StatsCommands(CalculatorEngine engine)
		{
			this.engine = engine;
			ClearStats();
		}

		[Command]
		public void ClearStats()
		{
			mean = Number.NaN;
			stdev = Number.NaN;
			statCount = 0;
		}

		[Command]
		public void StatsAdd()
		{
			if (statCount == 0)
			{
				mean = 0;
				stdev = 0;
			}
			statCount++;

			Number delta = engine.X - mean;
			mean += delta / statCount;
			stdev += delta * (engine.X - mean);
		}

		[Command]
		public void StatsSubtract()
		{
			if (statCount == 0)
			{
				mean = 0;
				stdev = 0;
			}
			statCount++;

			Number delta = -engine.X - mean;
			mean += delta / statCount;
			stdev += delta * (-engine.X - mean);
		}

		[Command]
		public void StatsMean()
		{
			engine.Push(mean);
		}

		[Command]
		public void StatsCount()
		{
			engine.Push(statCount);
		}

		[Command]
		public void StatsTotal()
		{
			engine.Push(statCount == 0 ? 0 : mean * statCount);
		}

		[Command]
		public void StatsSampleStdDev()
		{
			engine.Push(Number.Sqrt(stdev / (statCount - 1)));
		}

		[Command]
		public void StatsStdDev()
		{
			engine.Push(Number.Sqrt(stdev / statCount));
		}

		private Real NextReal()
		{
			return Real.NextReal(rand);
		}

		[Command]
		public void UniformRandom()
		{
			Real r = NextReal();
			if (engine.NumberSystem == NumberSystem.Binary)
				r *= Real.Pow(Real.Two, engine.WordSize);
			engine.Push(r);
		}

		[Command]
		public void GaussianRandom()
		{
			engine.Push(Real.Sqrt(-Real.Two * Real.Log(NextReal())) * Real.Cos(Real.Pi * NextReal()));
		}

		[Command]
		public void NormDist()
		{
			engine.UnaryOperation(delegate(Number x)
			{
				if (!x.IsReal())
					return Number.NaN;

				Number e = Number.Erf(x / Number.Sqrt(2));
				Number p = (1 + e) / 2;

				p.Re.SignificantBits = Math.Min(89, x.Re.SignificantBits);
				p.Im = 0;
				return p;
			});
		}

		[Command]
		public void NormInv()
		{
			engine.UnaryOperation(delegate(Number p)
			{
				if (!p.IsReal())
					return Number.NaN;

				if (p.Re < 0)
					return Number.NaN;

				if (p.Re > 1)
					return Number.NaN;

				// method due to Peter J. Acklam

				Number a1 = -39.69683028665376;
				Number a2 = 220.9460984245205;
				Number a3 = -275.9285104469687;
				Number a4 = 138.3577518672690;
				Number a5 = -30.66479806614716;
				Number a6 = 2.506628277459239;

				Number b1 = -54.47609879822406;
				Number b2 = 161.5858368580409;
				Number b3 = -155.6989798598866;
				Number b4 = 66.80131188771972;
				Number b5 = -13.28068155288572;

				Number c1 = -0.007784894002430293;
				Number c2 = -0.3223964580411365;
				Number c3 = -2.400758277161838;
				Number c4 = -2.549732539343734;
				Number c5 = 4.374664141464968;
				Number c6 = 2.938163982698783;

				Number d1 = 0.007784695709041462;
				Number d2 = 0.3224671290700398;
				Number d3 = 2.445134137142996;
				Number d4 = 3.754408661907416;

				// Define break-points.

				Number p_low = 0.02425;
				Number p_high = 1 - p_low;

				// Rational approximation for lower region.

				Number x;
				if (p < p_low)
				{
					Number q = Number.Sqrt(-2 * Number.Ln(p));
					x = (((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / 
						((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
				}

				// Rational approximation for central region.

				else if (p <= p_high)
				{
					Number q = p - 0.5;
					Number r = q * q;
					x = (((((a1 * r + a2) * r + a3) * r + a4) * r + a5) * r + a6) * q /
						(((((b1 * r + b2) * r + b3) * r + b4) * r + b5) * r + 1);
				}

				// Rational approximation for upper region.

				else
				{
					Number q = Number.Sqrt(-2 * Number.Ln(1 - p));
					x = -(((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) /
						((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
				}

				// refinement

				Number e = 0.5 * (1 - Number.Erf(-x / Number.Sqrt(2))) - p;
				Number u = e * Number.Sqrt(2 * Number.Pi) * Number.Exp(x * x / 2);
				x = x - u / (1 + x * u / 2);

				x.Re.SignificantBits = Math.Min(89, p.Re.SignificantBits);
				x.Im = 0;
				return x;
			}
			);
		}
	}
}
