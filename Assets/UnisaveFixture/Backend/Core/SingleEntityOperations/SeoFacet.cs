using Unisave.Facades;
using Unisave.Facets;

namespace UnisaveFixture.Backend.Core.SingleEntityOperations
{
    public class SeoFacet : Facet
    {
        public void SetUp()
        {
            var entities = DB.TakeAll<SeoEntity>().Get();
            foreach (var e in entities)
                e.Delete();
        }
        
        public void NonExistingEntityCannotBeFound()
        {
            var entity = DB.First<SeoEntity>();
            
            Assert.IsNull(entity);
        }

        public void EntityCanBeCreatedQueriedAndDeleted()
        {
            var e = new SeoEntity();
            Assert.IsNull(DB.First<SeoEntity>());
            e.Save();
            Assert.IsNotNull(DB.First<SeoEntity>());
            
            e.Delete();
            Assert.IsNull(DB.First<SeoEntity>());
        }

        public void EntityCanBeFoundById()
        {
            var e = new SeoEntity();
            e.Save();
            
            Assert.IsNotNull(DB.Find<SeoEntity>(e.EntityId));
        }

        public void EntityCanBeFoundByStringAttribute()
        {
            var a = new SeoEntity { StringAttribute = "A" };
            var b = new SeoEntity { StringAttribute = "B" };
            var c = new SeoEntity { StringAttribute = "C" };
            a.Save();
            b.Save();
            c.Save();

            var e = DB.TakeAll<SeoEntity>()
                .Filter(entity => entity.StringAttribute == "B")
                .First();
            
            Assert.IsNotNull(e);
            Assert.AreEqual("B", e.StringAttribute);
        }
        
        public void EntityCanBeFoundByEnumAttribute()
        {
            var a = new SeoEntity { EnumAttribute = SeoEntity.SeoEnum.A };
            var b = new SeoEntity { EnumAttribute = SeoEntity.SeoEnum.B };
            var c = new SeoEntity { EnumAttribute = SeoEntity.SeoEnum.C };
            a.Save();
            b.Save();
            c.Save();

            var e = DB.TakeAll<SeoEntity>()
                .Filter(entity => entity.EnumAttribute == SeoEntity.SeoEnum.B)
                .First();
            
            Assert.IsNotNull(e);
            Assert.AreEqual(
                (int)SeoEntity.SeoEnum.B,
                (int)e.EnumAttribute
            );
        }
    }
}