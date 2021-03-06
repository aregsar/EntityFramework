// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntry<TEntity, TProperty> : PropertyEntry where TEntity : class
    {
        public PropertyEntry([NotNull] StateEntry stateEntry, [NotNull] string name)
            : base(stateEntry, name)
        {
        }
    }
}
