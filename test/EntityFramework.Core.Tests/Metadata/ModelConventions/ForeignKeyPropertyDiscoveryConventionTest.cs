// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class ForeignKeyPropertyDiscoveryConventionTest
    {
        private readonly InternalModelBuilder _model = BuildModel();

        [Fact]
        public void Does_not_override_explicit_foreign_key_created_using_given_property()
        {
            DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);
            var fkProperty = DependentType.Property(typeof(int), "No!No!", ConfigurationSource.Convention).Metadata;

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                "SomeNav",
                null,
                ConfigurationSource.Explicit,
                oneToOne: false)
                .ForeignKey(new[] { fkProperty }, ConfigurationSource.Explicit);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.ForeignKeys.Single());
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Does_not_override_explicit_composite_foreign_key_created_using_given_properties()
        {
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);
            var fkProperty1 = DependentTypeWithCompositeKey.Property(typeof(int), "No!No!", ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(typeof(string), "No!No!2", ConfigurationSource.Convention);
            fkProperty2.Required(true, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                ConfigurationSource.Explicit,
                false)
                .ForeignKey(new[] { fkProperty1.Metadata, fkProperty2.Metadata }, ConfigurationSource.Explicit);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single());
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Returns_same_builder_if_no_matching_clr_properties_found()
        {
            DependentType.Property(DependentEntity.SomeNavIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.IDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                "SomeNav",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.ForeignKeys.Single());
            Assert.Equal("SomeNav" + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.False(fk.IsUnique);
            Assert.False(fk.IsRequired);
        }

        [Fact]
        public void Matches_navigation_plus_PK_name_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            InternalEntityBuilder dependentEntityTypeBuilder = DependentType;
            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                PrincipalType,
                dependentEntityTypeBuilder,
                "SomeNav",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Matches_navigation_plus_PK_name_properties()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropNameProperty, ConfigurationSource.Convention);
            fkProperty2.Required(true, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);

            InternalEntityBuilder dependentEntityTypeBuilder = DependentTypeWithCompositeKey;
            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                PrincipalTypeWithCompositeKey,
                dependentEntityTypeBuilder,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Matches_principal_type_plus_PK_name_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                "SomeNav",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Matches_principal_type_plus_PK_name_properties()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyIdProperty, ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            fkProperty2.Required(true, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Matches_PK_name_property()
        {
            var fkProperty = DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention).Metadata;

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                "SomeNav",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Matches_PK_name_properties()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);
            var fkProperty2 = DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Convention);
            fkProperty2.Required(true, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1.Metadata, fk.Properties[0]);
            Assert.Same(fkProperty2.Metadata, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Matches_dependent_PK_for_unique_FK()
        {
            var fkProperty = DependentType.Metadata.GetPrimaryKey().Properties.Single();

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                "SomeNav",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentType.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(PrimaryKey, fk.ReferencedProperties.Single());
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Does_not_match_dependent_PK_for_non_unique_FK()
        {
            DependentType.PrimaryKey(new[] { DependentEntity.PrincipalEntityPeEKaYProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                null,
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentType.Metadata.ForeignKeys.Single());
            Assert.Equal(PrincipalType.Metadata.SimpleName + PrimaryKey.Name, fk.Properties.Single().Name);
            Assert.False(fk.IsUnique);
            Assert.False(fk.IsRequired);
        }

        [Fact]
        public void Matches_composite_dependent_PK_for_unique_FK()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Metadata.GetPrimaryKey().Properties[0];
            var fkProperty2 = DependentTypeWithCompositeKey.Metadata.GetPrimaryKey().Properties[1];

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            var newRelationshipBuilder = new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, newRelationshipBuilder);

            var fk = (IForeignKey)DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single();
            Assert.Same(fk, newRelationshipBuilder.Metadata);
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.True(fk.IsUnique);
            Assert.True(fk.IsRequired);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_non_unique_FK()
        {
            DependentTypeWithCompositeKey.PrimaryKey(
                new[] { DependentEntityWithCompositeKey.NavPropIdProperty, DependentEntityWithCompositeKey.NavPropNameProperty },
                ConfigurationSource.Explicit);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single());
            Assert.Equal("NavProp" + CompositePrimaryKey[0].Name + "1", fk.Properties[0].Name);
            Assert.Equal("NavProp" + CompositePrimaryKey[1].Name + "1", fk.Properties[1].Name);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.False(fk.IsUnique);
            Assert.False(fk.IsRequired);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_unique_FK_if_count_mismatched()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Metadata.GetPrimaryKey().Properties[0];
            DependentTypeWithCompositeKey.PrimaryKey(new[] { fkProperty1.Name }, ConfigurationSource.Explicit);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single());
            Assert.Equal(2, fk.Properties.Count);
            Assert.NotSame(fkProperty1, fk.Properties[0]);
            Assert.NotSame(fkProperty1, fk.Properties[1]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);
        }

        [Fact]
        public void Does_not_match_composite_dependent_PK_for_unique_FK_if_order_mismatched()
        {
            var fkProperty1 = DependentTypeWithCompositeKey.Metadata.GetPrimaryKey().Properties[0];
            var fkProperty2 = DependentTypeWithCompositeKey.Metadata.GetPrimaryKey().Properties[1];
            DependentTypeWithCompositeKey.PrimaryKey(new[] { fkProperty2.Name, fkProperty1.Name }, ConfigurationSource.Explicit);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                true);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single());
            Assert.Equal(2, fk.Properties.Count);
            Assert.NotSame(fkProperty1, fk.Properties[0]);
            Assert.NotSame(fkProperty2, fk.Properties[1]);
            Assert.NotSame(fkProperty1, fk.Properties[1]);
            Assert.NotSame(fkProperty2, fk.Properties[0]);
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.True(fk.IsUnique);
            Assert.False(fk.IsRequired);
        }

        [Fact]
        public void Does_not_match_properties_with_different_base_names()
        {
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NavPropIdProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.PrincipalEntityWithCompositeKeyNameProperty, ConfigurationSource.Convention);
            DependentTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.IdProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentTypeWithCompositeKey.Relationship(
                PrincipalTypeWithCompositeKey,
                DependentTypeWithCompositeKey,
                "NavProp",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fk, DependentTypeWithCompositeKey.Metadata.ForeignKeys.Single());
            Assert.Same(CompositePrimaryKey[0], fk.ReferencedProperties[0]);
            Assert.Same(CompositePrimaryKey[1], fk.ReferencedProperties[1]);
            Assert.False(fk.IsUnique);
            Assert.False(fk.IsRequired);
        }

        [Fact]
        public void Does_nothing_if_a_foreign_key_on_the_best_candidate_property_already_exists()
        {
            var fkProperty = DependentType.Property(DependentEntity.SomeNavPeEKaYProperty, ConfigurationSource.Convention).Metadata;
            DependentType.Property(DependentEntity.PrincipalEntityIDProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PrincipalEntityPeEKaYProperty, ConfigurationSource.Convention);
            DependentType.Property(DependentEntity.PeEKaYProperty, ConfigurationSource.Convention);

            var relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                null,
                null,
                new[] { fkProperty },
                PrincipalType.Metadata.GetPrimaryKey().Properties,
                ConfigurationSource.Convention,
                false);

            var fk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.False(fk.IsUnique);
            Assert.True(fk.IsRequired);

            relationshipBuilder = DependentType.Relationship(
                PrincipalType,
                DependentType,
                "SomeNav",
                null,
                null,
                null,
                ConfigurationSource.Convention,
                false);

            Assert.Same(relationshipBuilder, new ForeignKeyPropertyDiscoveryConvention().Apply(relationshipBuilder));

            var newFk = (IForeignKey)relationshipBuilder.Metadata;
            Assert.Equal(2, DependentType.Metadata.ForeignKeys.Count);
            Assert.NotEqual(fkProperty.Name, newFk.Properties.Single().Name);
            Assert.False(newFk.IsUnique);
            Assert.False(newFk.IsRequired);
        }

        private static InternalModelBuilder BuildModel()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var principalType = modelBuilder.Entity(typeof(PrincipalEntity), ConfigurationSource.Explicit);
            principalType.PrimaryKey(new[] { "PeeKay" }, ConfigurationSource.Explicit);

            var dependentType = modelBuilder.Entity(typeof(DependentEntity), ConfigurationSource.Explicit);
            dependentType.PrimaryKey(new[] { "KayPee" }, ConfigurationSource.Explicit);

            var principalTypeWithCompositeKey = modelBuilder.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Explicit);
            principalTypeWithCompositeKey.PrimaryKey(new[] { "Id", "Name" }, ConfigurationSource.Explicit);
            principalTypeWithCompositeKey.Property(DependentEntityWithCompositeKey.NameProperty, ConfigurationSource.Explicit).Required(true, ConfigurationSource.Explicit);

            var dependentTypeWithCompositeKey = modelBuilder.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Explicit);
            dependentTypeWithCompositeKey.PrimaryKey(new[] { "NotId", "NotName" }, ConfigurationSource.Explicit);
            dependentTypeWithCompositeKey.Property(typeof(string), "NotName", ConfigurationSource.Explicit).Required(true, ConfigurationSource.Explicit);

            return modelBuilder;
        }

        private Property PrimaryKey
        {
            get { return PrincipalType.Metadata.GetPrimaryKey().Properties.Single(); }
        }

        private InternalEntityBuilder PrincipalType => _model.Entity(typeof(PrincipalEntity), ConfigurationSource.Convention);

        private InternalEntityBuilder DependentType => _model.Entity(typeof(DependentEntity), ConfigurationSource.Convention);

        private IReadOnlyList<Property> CompositePrimaryKey
        {
            get { return PrincipalTypeWithCompositeKey.Metadata.GetPrimaryKey().Properties; }
        }

        private InternalEntityBuilder PrincipalTypeWithCompositeKey => _model.Entity(typeof(PrincipalEntityWithCompositeKey), ConfigurationSource.Convention);

        private InternalEntityBuilder DependentTypeWithCompositeKey => _model.Entity(typeof(DependentEntityWithCompositeKey), ConfigurationSource.Convention);

        private class PrincipalEntity
        {
            public int PeeKay { get; set; }
            public IEnumerable<DependentEntity> InverseNav { get; set; }
        }

        private class DependentEntity
        {
            public static readonly PropertyInfo SomeNavIDProperty = typeof(DependentEntity).GetProperty("SomeNavID");
            public static readonly PropertyInfo SomeNavPeEKaYProperty = typeof(DependentEntity).GetProperty("SomeNavPeEKaY");
            public static readonly PropertyInfo PrincipalEntityIDProperty = typeof(DependentEntity).GetProperty("PrincipalEntityID");
            public static readonly PropertyInfo PrincipalEntityPeEKaYProperty = typeof(DependentEntity).GetProperty("PrincipalEntityPeEKaY");
            public static readonly PropertyInfo IDProperty = typeof(DependentEntity).GetProperty("ID");
            public static readonly PropertyInfo PeEKaYProperty = typeof(DependentEntity).GetProperty("PeEKaY");

            public int KayPee { get; set; }

            public int SomeNavID { get; set; }
            public int SomeNavPeEKaY { get; set; }
            public int PrincipalEntityID { get; set; }
            public int PrincipalEntityPeEKaY { get; set; }
            public int ID { get; set; }
            public int PeEKaY { get; set; }

            public PrincipalEntity SomeNav { get; set; }
        }

        private class PrincipalEntityWithCompositeKey
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<DependentEntityWithCompositeKey> InverseNav { get; set; }
        }

        private class DependentEntityWithCompositeKey
        {
            public static readonly PropertyInfo NavPropIdProperty = typeof(DependentEntityWithCompositeKey).GetProperty("NavPropId");
            public static readonly PropertyInfo NavPropNameProperty = typeof(DependentEntityWithCompositeKey).GetProperty("NavPropName");
            public static readonly PropertyInfo PrincipalEntityWithCompositeKeyIdProperty = typeof(DependentEntityWithCompositeKey).GetProperty("PrincipalEntityWithCompositeKeyId");
            public static readonly PropertyInfo PrincipalEntityWithCompositeKeyNameProperty = typeof(DependentEntityWithCompositeKey).GetProperty("PrincipalEntityWithCompositeKeyName");
            public static readonly PropertyInfo IdProperty = typeof(DependentEntityWithCompositeKey).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(DependentEntityWithCompositeKey).GetProperty("Name");

            public int NotId { get; set; }
            public string NotName { get; set; }

            public int NavPropId { get; set; }
            public string NavPropName { get; set; }
            public int PrincipalEntityWithCompositeKeyId { get; set; }
            public string PrincipalEntityWithCompositeKeyName { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }

            public PrincipalEntityWithCompositeKey NavProp { get; set; }
        }
    }
}
