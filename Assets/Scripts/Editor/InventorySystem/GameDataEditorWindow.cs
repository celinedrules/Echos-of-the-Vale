// Done
using System.Linq;
using Data.DialogueData;
using Data.EntityData;
using Data.ItemData;
using Data.QuestData;
using Data.SkillData;
using Editor.Drawers.DialogueTable;
using InventorySystem;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.InventorySystem
{
    public class GameDataEditorWindow : OdinMenuEditorWindow
    {
        public override bool ResizableMenuWidth => false;
        public override float MenuWidth { get; set; } = 275f;

        private readonly DialogueTableDrawer _dialogueTableDrawer = new();


        [MenuItem("Tools/Game Data Editor")]
        private static void Open()
        {
            GameDataEditorWindow window = GetWindow<GameDataEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
            window.minSize = new Vector2(725, 600);
        }

        protected override void Initialize()
        {
            base.Initialize();
            _dialogueTableDrawer.HostWindow = this;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(true)
            {
                DefaultMenuStyle =
                {
                    IconSize = 28.0f
                },
                Config =
                {
                    DrawSearchToolbar = true
                }
            };


            tree.AddAllAssetsAtPath("Players", "Assets/Data/Players", typeof(PlayerData), true, true).ForEach(AddDragHandles);
            tree.AddAllAssetsAtPath("Npcs", "Assets/Data/Npcs", typeof(NpcData), true).ForEach(AddDragHandles);
            tree.AddAllAssetsAtPath("Enemies", "Assets/Data/Enemies", typeof(EnemyData), true, true).ForEach(AddDragHandles);

            tree.AddAllAssetsAtPath("Items", "Assets/Data/Items", typeof(ItemData), true).ForEach(AddDragHandles);
            tree.AddAllAssetsAtPath("Skills", "Assets/Data/Skills", typeof(SkillData), true).ForEach(AddDragHandles);

            // Replace folder-driven quest listing with a parent->children quest tree.
            QuestMenuTreeBuilder.AddQuests(tree, "Quests", "Assets/Data/Quests", AddDragHandles);

            // Dialogue tables
            DialogueMenuTreeBuilder.AddDialogueTables(tree, "Dialogues", "Assets/Data/Dialogues", AddDragHandles);

            tree.EnumerateTree().Where(x => x.Value is InventoryItem).ForEach(AddDragHandles);

            tree.EnumerateTree().AddIcons<PlayerData>(x => x.Icon);
            tree.EnumerateTree().AddIcons<NpcData>(x => x.Icon);
            tree.EnumerateTree().AddIcons<EnemyData>(x => x.Icon);
            tree.EnumerateTree().AddIcons<ItemData>(x => x.ItemIcon);
            tree.EnumerateTree().AddIcons<QuestData>(x => x.Icon);
            tree.EnumerateTree().AddIcons<DialogueTable>(t =>
                t.FirstRow?.Speaker != null ? t.FirstRow.Speaker.SpeakerPortrait : null);

            return tree;
        }

        private void AddDragHandles(OdinMenuItem menuItem)
        {
            menuItem.OnDrawItem += _ => DragAndDropUtilities.DragZone(menuItem.Rect, menuItem.Value, false, false);
        }

        protected override void DrawEditors()
        {
            OdinMenuItem selected = MenuTree.Selection.FirstOrDefault();

            // If a DialogueTable is selected, draw the custom table editor instead of the default inspector.
            if (selected?.Value is DialogueTable table)
            {
                _dialogueTableDrawer.SetTable(table);
                _dialogueTableDrawer.Draw();
                return;
            }

            // For everything else, draw the default Odin inspector.
            base.DrawEditors();
        }

        protected override void OnBeginDrawEditors()
        {
            OdinMenuItem selected = MenuTree.Selection.FirstOrDefault();
            int toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                    GUILayout.Label(selected.Name);
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Dialogue")))
                {
                    ScriptableObjectCreator.ShowDialog<DialogueTable>("Assets/Data/Dialogues", obj =>
                    {
                        obj.name = "New Dialogue Table";
                        TrySelectMenuItemWithObject(obj);
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Quest")))
                {
                    ScriptableObjectCreator.ShowDialog<QuestData>("Assets/Data/Quests", obj =>
                    {
                        obj.name = obj.QuestName;
                        TrySelectMenuItemWithObject(obj);
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Item")))
                {
                    ScriptableObjectCreator.ShowDialog<ItemData>("Assets/Data/Items", obj =>
                    {
                        obj.name = obj.ItemName;
                        TrySelectMenuItemWithObject(obj);
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Skill")))
                {
                    ScriptableObjectCreator.ShowDialog<SkillData>("Assets/Data/Skills", obj =>
                    {
                        obj.name = obj.DisplayName;
                        TrySelectMenuItemWithObject(obj);
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Enemy")))
                {
                    ScriptableObjectCreator.ShowDialog<EnemyData>("Assets/Data/Enemies",
                        obj =>
                        {
                            //obj.name = obj.EntityName;
                            TrySelectMenuItemWithObject(obj);
                        });
                }
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Npc")))
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Merchant"), false, () =>
                    {
                        ScriptableObjectCreator.ShowDialog<MerchantNpcData>("Assets/Data/Npcs/Merchants",
                            obj => TrySelectMenuItemWithObject(obj));
                    });

                    menu.AddItem(new GUIContent("Blacksmith"), false, () =>
                    {
                        ScriptableObjectCreator.ShowDialog<BlacksmithNpcData>("Assets/Data/Npcs/Blacksmiths",
                            obj => TrySelectMenuItemWithObject(obj));
                    });
                    
                    menu.AddItem(new GUIContent("DialogueOnly"), false, () =>
                    {
                        ScriptableObjectCreator.CreateExact<NpcData>("Assets/Data/Npcs/DialogueOnly",
                            obj => TrySelectMenuItemWithObject(obj));
                    });

                    menu.ShowAsContext();
                }
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Player")))
                {
                    ScriptableObjectCreator.ShowDialog<PlayerData>("Assets/Data/Players",
                        obj =>
                        {
                            //obj.name = obj.EntityName;
                            TrySelectMenuItemWithObject(obj);
                        });
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}