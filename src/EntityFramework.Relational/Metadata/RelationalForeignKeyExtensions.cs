// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalForeignKeyExtensions : ReadOnlyRelationalForeignKeyExtensions
    {
        public RelationalForeignKeyExtensions([NotNull] ForeignKey foreignKey)
            : base(foreignKey)
        {
        }

        public new virtual string Name
        {
            get { return base.Name; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((ForeignKey)ForeignKey)[NameAnnotation] = value;
            }
        }
    }
}
