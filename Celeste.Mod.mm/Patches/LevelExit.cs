﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0169 // The field is never used
#pragma warning disable CS0414 // The field is assigned but its value is never used

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod;
using MonoMod;
using System.Collections;
using Monocle;
using System.IO;
using System.Xml;
using Celeste.Mod.Meta;

namespace Celeste {
    class patch_LevelExit : LevelExit {

        // We're effectively in LevelExit, but still need to "expose" private fields to our mod.
        private Session session;
        private XmlElement completeXml;
        private Atlas completeAtlas;
        private bool completeLoaded;

        private MapMetaCompleteScreen completeMeta;

        public patch_LevelExit(Mode mode, Session session, HiresSnow snow = null)
            : base(mode, session, snow) {
            // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
        }

        public extern void orig_ctor(Mode mode, Session session, HiresSnow snow = null);
        [MonoModConstructor]
        public void ctor(Mode mode, Session session, HiresSnow snow = null) {
            // Restore to metadata of A-Side.
            AreaData.Get(session).RestoreASideAreaData();
            
            orig_ctor(mode, session, snow);
            Everest.Events.Level.Exit(Engine.Scene as Level, this, mode, session, snow);
        }

        [MonoModReplace]
        private void LoadCompleteThread() {
            AreaData area = AreaData.Get(session);

            if ((completeMeta = area.GetMeta()?.CompleteScreen) != null && completeMeta.Atlas != null) {
                completeAtlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", completeMeta.Atlas), Atlas.AtlasDataFormat.PackerNoAtlas);

            } else if ((completeXml = area.CompleteScreenXml) != null && completeXml.HasAttr("atlas")) {
                completeAtlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", completeXml.Attr("atlas")), Atlas.AtlasDataFormat.PackerNoAtlas);
            }

            completeLoaded = true;
        }

        [MonoModIgnore] // We don't want to change anything about the method...
        [PatchLevelExitRoutine] // ... except for slapping an additional parameter to / updating newobj AreaComplete
        private extern IEnumerator Routine();

    }
}
