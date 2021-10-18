// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Threading;
using Unreal.CoreUObject;
using Unreal.Engine;

namespace Unreal
{
    [UClass, Blueprintable]
    public partial class UCustomObjectBase : UObject
    {
        private int m_talkedToTimes = 0;

        private static int m_idCounter = 1;

        public int Id { get; private set; } = 0;

        protected Color FavoriteColour = Color.Black;

        [Constructor]
        private void Construct()
        {
            Id = Interlocked.Increment(ref m_idCounter);
        }

        [UFunction, BlueprintCallable]
        public void TalkToAnother(UCustomObjectBase other)
        {
            other.m_talkedToTimes++;
        }

        [UFunction, BlueprintCallable]
        public void TellHowManyTalks()
        {
            UEngine.Instance.PrintMessage(-1, 5, FavoriteColour,
                $"Hello there, my name is {Name}, my id is {Id}, and I have been talked to {m_talkedToTimes} time(s).");
        }

        protected virtual string Name => "Noname";
    }
}