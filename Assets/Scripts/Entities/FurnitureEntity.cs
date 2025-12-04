using Entities.Molds;

namespace Entities
{
    public class FurnitureEntity: RigidbodyEntity
    {
        public FurnitureMold SourceMold { get; private set; }
        
        public override void LoadEntity(Mold entityMold)
        {
            base.LoadEntity(entityMold);
            
            SourceMold = entityMold as FurnitureMold;
            SwitchGraphics(true);
        }
    }
}