/* -*- mode: csharp; c-basic-offset: 2 -*-
 * Copyright 2013 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 
 Modified to version b0006 on 2/26/2014 by Lloyd E.Sponenburgh, to fix
 the shaded icon problem in CamBam when the MOp is disabled.
 Look for the "modified" comments
 */
 /* On 3/10/14, MacBob found another one.  When Paul processed a segment too short to do any math
 upon, he issued a warning message, but didn't skip that segment.  As a result, he ended up
 with an N/A value in the structure "dse" down in the FollowOutline routine.Look for the "OOPs!" in
 FollowOutline in Plugin.cs (this file).
 */


 using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

using CamBam;
using CamBam.CAD;
using CamBam.CAM;
using CamBam.UI;
using CamBam.Library;


//[assembly: InternalsVisibleTo("VEngrave_UnitTests")]

namespace VEngraveForCamBam
{
    public class Plugin
    {
        private CamBamUI _ui;
        private Logger _log = new CamBamLogger();
        public Plugin() : this(CamBamUI.MainUI) { }

        public Plugin(CamBamUI ui)
        {
            _ui = ui;
            AttachToUI();
        }

        private void AttachToUI()
        {
            var aboutCommand = new ToolStripMenuItem();
            aboutCommand.Text = "V-Engrave About";
            aboutCommand.Click += About;
            _ui.Menus.mnuPlugins.DropDownItems.Add(aboutCommand);

            var insertMOPCommand = new ToolStripMenuItem();
            insertMOPCommand.Text = "V-Engrave";
            // modified 2/26/2014 by L.E.S. for the "ON" image fname1.img
            insertMOPCommand.Image = Properties.VEngraveResources.cam_VEngraveButton1;
            //  end modification 2/16/2014
            insertMOPCommand.Click += InsertMOP;

            for (int i = 0; i < _ui.Menus.mnuMachining.DropDownItems.Count; ++i)
            {
                var item = _ui.Menus.mnuMachining.DropDownItems[i];
                if (item is ToolStripSeparator || i == _ui.Menus.mnuMachining.DropDownItems.Count - 1)
                {
                    _ui.Menus.mnuMachining.DropDownItems.Insert(i, insertMOPCommand);
                    break;
                }
            }

            insertMOPCommand = insertMOPCommand = new ToolStripMenuItem();
            insertMOPCommand.Text = "V-Engrave";
            // modified 2/26/2014 by L.E.S. for the "ON" image fname1.img
            insertMOPCommand.Image = Properties.VEngraveResources.cam_VEngraveButton1;
            //  end modification 2/16/2014
            insertMOPCommand.Click += InsertMOP;

            foreach (ToolStripItem item in _ui.ViewContextMenus.ViewContextMenu.Items)
            {
                if (item is ToolStripMenuItem && item.Name == "machineToolStripMenuItem")
                {
                    ToolStripMenuItem menu = (ToolStripMenuItem)item;
                    for (int i = 0; i < menu.DropDownItems.Count; ++i)
                    {
                        if (menu.DropDownItems[i] is ToolStripSeparator || i == menu.DropDownItems.Count - 1)
                        {
                            menu.DropDownItems.Insert(i, insertMOPCommand);
                            break;
                        }
                    }
                    break;
                }
            }

            // Hook into toolbar?
            // Hook into XML serialization

            if (CADFile.ExtraTypes == null) { CADFile.ExtraTypes = new List<Type>(); }
            CADFile.ExtraTypes.Add(typeof(MOPVEngrave));

            // HACK: Per 10bulls, this fixes clipboard errors associated with
            // copying external assembly objects that have not been serialized.
            new XmlSerializer(typeof(MOPVEngrave)).Serialize(new MemoryStream(), new MOPVEngrave());
        }



        public static void InitPlugin(CamBamUI ui)
        {
            Plugin plugin = new Plugin(ui);
        }

        private void About(object sender, EventArgs e)
        {
            // ThisApplication.MsgBox("VEngrave CamBam plug-in b0010");

            // EddyCurrent V-Engrave About Form
            Form1 uiForm;
            uiForm = new Form1();
            uiForm.Owner = CamBam.ThisApplication.TopWindow;
            uiForm.Text = "V-Engrave b0012_03";
            uiForm.ShowDialog();
            // EddyCurrent
        }

        private void InsertMOP(object sender, EventArgs e)
        {
            ICADView view = CamBamUI.MainUI.ActiveView;
            CADFile file = view.CADFile;
            object[] objects = view.SelectedEntities;

            MOPVEngrave mop = new MOPVEngrave(file, objects);
            CAMPart part = view.CADFile.EnsureActivePart(true);

            mop.Part = part;
            // added by EddyCurrent 17/04/2021 //
            // setting MaxCrossoverDistance to 0 prevents the tool leaving a trail between letters //
            mop.MaxCrossoverDistance = new CamBam.Values.CBValue<double>(0.0);
            _ui.InsertMOP(mop);

            // part.MachineOps.Add(mop);
            file.Modified = true;

            ToolDefinition mytool = new ToolDefinition();
            mytool = mop.CurrentTool;

            // verify these
            CamBamUI.MainUI.CADFileTree.Machining.Expand();
            foreach (TreeNode partNode in view.DrawingTree.Machining.Nodes)
            {
                if (partNode.Tag == view.CADFile.ActivePart)
                {
                    partNode.Expand();
                    foreach (TreeNode opNode in partNode.Nodes)
                    {
                        if (opNode.Tag == mop)
                        {
                            mop.Name = opNode.Text;
                            opNode.EnsureVisible();
                            break;
                        }
                    }
                    break;
                }
            }

            foreach (var smop in CamBamUI.MainUI.CADFileTree.SelectedMOPs)
            {
                ThisApplication.AddLogMessage("SelectedMOPs: {0}", smop.Name);
            }
        }
    }
}

