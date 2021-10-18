// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal
{
    [UClass, Blueprintable]
    public partial class UCustomObjectAngela : UCustomObjectBase
    {
        protected override string Name => "Angela";

        [Constructor]
        private void Construct()
        {
            FavoriteColour = Color.Cyan;
        }
    }
}