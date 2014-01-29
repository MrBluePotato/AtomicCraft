// Copyright 2009-2014 Matvei Stefarov <me@matvei.org>

namespace fCraft.Drawing
{
    internal sealed class QuickPasteDrawOperation : PasteDrawOperation
    {
        public QuickPasteDrawOperation(Player player, bool not)
            : base(player, not)
        {
        }

        public override string Name
        {
            get { return Not ? "PasteNot" : "Paste"; }
        }

        public override bool Prepare(Vector3I[] marks)
        {
            return base.Prepare(new[] {marks[0], marks[0]});
        }
    }
}