// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal
{
    [UClass, Blueprintable]
    public partial class UCustomObjectKevin : UCustomObjectBase
    {
        protected override string Name => "Kevin";
        
        [Constructor]
        private void Construct()
        {
            FavoriteColour = Color.Orange;
        }
    }
}