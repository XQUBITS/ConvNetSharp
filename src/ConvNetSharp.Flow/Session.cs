﻿using System;
using System.Collections.Generic;
using ConvNetSharp.Flow.Graph;
using ConvNetSharp.Flow.Ops;
using ConvNetSharp.Volume;

namespace ConvNetSharp.Flow
{
    /// <summary>
    /// TODO:
    /// - release allocations on Dispose
    /// - scope management (to group ops together and to allow using the same name on different nodes)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Session<T> : IDisposable where T : struct, IEquatable<T>, IFormattable
    {
        private bool _derivativeComputed;
        public Op<T> Cost { get; private set; }

        public Dictionary<string, Variable<T>> LearnableVariables { get; set; } = new Dictionary<string, Variable<T>>();

        public void Dispose()
        {
            var visitor = new OpVisitor<T>(op =>
            {
                op.Dispose();
            });
            this.Cost?.Accept(visitor);
        }

        /// <summary>
        /// Automatic differentiation using reverse accumulation
        /// </summary>
        /// <param name="cost"></param>
        public void Differentiate(Op<T> cost)
        {
            if (!this._derivativeComputed)
            {
                this.Cost = cost;

                cost.Derivate = ConvNetSharp<T>.Const(ConvNetSharp<T>.One, "1");

                //this._func.Derivate = cost;
                var differentiateVisitor = new DifferentiateVisitor<T>();
                cost.Accept(differentiateVisitor);

                this._derivativeComputed = true;
            }
        }

        public Volume<T> Run(Op<T> fun, Dictionary<string, Volume<T>> dictionary)
        {
            // Find all PlaceHolders and update their current value
            var visitor = new OpVisitor<T>(op =>
            {
                var placeHolder = op as PlaceHolder<T>;
                if (placeHolder != null)
                {
                    placeHolder.V = dictionary[placeHolder.Name];
                }

                var variable = op as Variable<T>;
                if (variable != null)
                {
                    this.LearnableVariables[variable.Name] = variable;
                }
            });
            fun.Accept(visitor);

            var result = fun.Evaluate(this);

            this.Step++;

            return result;
        }

        public long Step { get; set; }
    }
}