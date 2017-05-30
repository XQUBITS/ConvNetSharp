﻿using System;
using ConvNetSharp.Flow;
using ConvNetSharp.Flow.Ops;

namespace ConvNetSharp.Core.Layers
{
    public class SoftmaxLayer<T> : LayerBase<T> where T : struct, IEquatable<T>, IFormattable
    {
        public override void AcceptParent(Op<T> parent)
        {
            this.Op = ConvNetSharp<T>.Softmax(parent);
        }
    }
}