using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;

namespace HexLoom
{
    enum PrimaryTypes : Int32
    {
        PRIMITIVE,
        ARRAY,
        STRING,
        COLOR
    };

    enum PrimitiveTypes : Int32
    {
        SINT8,
        UINT8,
        SINT16,
        UINT16,
        SINT32,
        UINT32,
        SINT64,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOL
    };

    enum ColorTypes : Int32
    {
        RGB,
        RGBA,
        RGBF,
        RGBAF
    };

    enum ArrayTypes : Int32
    {
        SINT8,
        UINT8,
        SINT16,
        UINT16,
        SINT32,
        UINT32,
        SINT64,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOL/*,
        RGB,
        RGBA,
        RGBF,
        RGBAF*/
    };

    enum StringTypes : Int32
    {
        UTF8 = 1,
        UTF16LE,
        UTF16BE,
        UTF32LE,
        UTF32BE,
        ASCII,
        ISO_8859_1,
        ISO_8859_2,
        ISO_8859_3,
        ISO_8859_4,
        ISO_8859_5,
        ISO_8859_6,
        ISO_8859_7,
        ISO_8859_8,
        ISO_8859_9,
        ISO_8859_10,
        ISO_8859_11,
        ISO_8859_13,
        ISO_8859_14,
        ISO_8859_15,
        ISO_8859_16,
        SHIFTJIS_CP932,
        JIS_X_0201_FULLWIDTH,
        JIS_X_0201_HALFWIDTH,
        KS_X_1001,
        Reserved,
        POKEMON_GEN1_ENGLISH,
        POKEMON_GEN1_FRENCH_GERMAN,
        POKEMON_GEN1_ITALIAN_SPANISH,
        POKEMON_GEN1_JAPANESE,
        POKEMON_GEN2_ENGLISH
    };

    public partial class Entity : ContentView
    {
        public Entity()
        {
            InitializeComponent();
            setupPirmaryTypePicker();
            setupSecondaryDatatypePicker((Int32)PrimaryTypes.PRIMITIVE);
            WidgetHelpers.SetWidgetPadding<Entry>(0, 0, 0, 0);
            WidgetHelpers.SetWidgetPadding<Microsoft.Maui.Controls.Button>(0, 0, 0, 0);
            WidgetHelpers.SetWidgetPadding<Picker>(0, 0, 0, 0);
            WidgetHelpers.SetWidgetPadding<Microsoft.Maui.Controls.CheckBox>(0, 0, 0, 0);
        }

        public Entity(Newtonsoft.Json.Linq.JObject json) : this()
        {
            _EntityName = json["EntityName"].ToString();
            _PrimaryType = (Int32)json["PrimaryType"];
            _SecondaryType = (Int32)json["SecondaryType"];
            _EntityOffset = (UInt64)json["Offset"];
            _Apply = (bool)json["Apply"];

            if (_PrimaryType == (Int32)PrimaryTypes.PRIMITIVE && _SecondaryType == (Int32)PrimitiveTypes.BOOL)
                _EntityValueBool = (bool)json["Value"];
            else
                _EntityValue = json["Value"].ToString();
        }

        public bool _Apply
        {
            get => this.ApplyCheckbox.IsChecked;
            set => this.ApplyCheckbox.IsChecked = value;
        }
        public Int32 _PrimaryType
        {
            get => ((PickerItem)this.PrimaryTypePicker.SelectedItem).Id;
            set => ((PickerItem)this.PrimaryTypePicker.SelectedItem).Id = value;
        }
        public Int32 _SecondaryType
        {
            get => ((PickerItem)this.SecondaryTypePicker.SelectedItem).Id;
            set => ((PickerItem)this.SecondaryTypePicker.SelectedItem).Id = value;
        }
        public UInt64 _EntityOffset { get; set; }
        public string _EntityName
        {
            get => this.EntryItemName.Text;
            set => this.EntryItemName.Text = value;
        }
        public string _EntityValue
        {
            get => this.EntryItemValueText.Text;
            set => this.EntryItemValueText.Text = value;
        }
        public bool _EntityValueBool
        {
            get => this.EntryItemValueBool.IsChecked;
            set => this.EntryItemValueBool.IsChecked = value;
        }

        private void onOffsetTextChanged(object sender, EventArgs e)
        {
            try
            {
                _EntityOffset = Convert.ToUInt64(this.EntryItemOffset.Text, 16);
            }
            catch (Exception)
            {
                _EntityOffset = 0;
            }

            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onActiveChanged(object sender, EventArgs e)
        {
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onEntityNameTextChanged(object sender, EventArgs e)
        {
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void setupPirmaryTypePicker()
        {
            var itemList = new List<PickerItem>
            {
                new PickerItem { Id = (Int32)PrimaryTypes.PRIMITIVE, DisplayName = "Primitive" },
                new PickerItem { Id = (Int32)PrimaryTypes.ARRAY, DisplayName = "Array" },
                new PickerItem { Id = (Int32)PrimaryTypes.STRING, DisplayName = "String" },
                new PickerItem { Id = (Int32)PrimaryTypes.COLOR, DisplayName = "Color" }
            };

            PrimaryTypePicker.ItemsSource = itemList;
            PrimaryTypePicker.ItemDisplayBinding = new Binding("DisplayName");
            PrimaryTypePicker.SelectedIndex = 0;
        }

        private void onPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            WidgetHelpers.PanUpdated(this, sender, e);
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onDeleteClicked(object sender, EventArgs e)
        {
            var parentStack = this.Parent as VerticalStackLayout;

            if (parentStack != null)
                parentStack.Children.Remove(this);

            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onSecondaryTypeIndexChanged(object sender, EventArgs e)
        {
            Int32 primaryType = ((PickerItem)PrimaryTypePicker.SelectedItem).Id;
            EntryItemValueBool.IsVisible = false;
            EntryItemValueText.IsVisible = true;

            if (primaryType != (Int32)PrimaryTypes.PRIMITIVE)
                return;

            Int32 secondaryType = ((PickerItem)SecondaryTypePicker.SelectedItem).Id;

            if(secondaryType == (Int32)PrimitiveTypes.BOOL)
            {
                EntryItemValueText.IsVisible = false;
                EntryItemValueBool.IsVisible = true;
            }

            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onPrimaryTypeIndexChanged(object sender, EventArgs e)
        {
            setupSecondaryDatatypePicker(PrimaryTypePicker.SelectedIndex);
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void setupSecondaryDatatypePicker(Int32 primaryType)
        {
            List<PickerItem> itemList = null;

            switch (primaryType)
            {
                case (Int32)PrimaryTypes.ARRAY:
                {
                    itemList = new List<PickerItem>
                    {
                        new PickerItem { Id = (Int32)ArrayTypes.SINT8, DisplayName = "Int 8" },
                        new PickerItem { Id = (Int32)ArrayTypes.UINT8, DisplayName = "UInt 8" },
                        new PickerItem { Id = (Int32)ArrayTypes.SINT16, DisplayName = "Int 16" },
                        new PickerItem { Id = (Int32)ArrayTypes.UINT16, DisplayName = "UInt 16" },
                        new PickerItem { Id = (Int32)ArrayTypes.SINT32, DisplayName = "Int 32" },
                        new PickerItem { Id = (Int32)ArrayTypes.UINT32, DisplayName = "UInt 32" },
                        new PickerItem { Id = (Int32)ArrayTypes.SINT64, DisplayName = "Int 64" },
                        new PickerItem { Id = (Int32)ArrayTypes.UINT64, DisplayName = "UInt 64" },
                        new PickerItem { Id = (Int32)ArrayTypes.FLOAT, DisplayName = "Float" },
                        new PickerItem { Id = (Int32)ArrayTypes.DOUBLE, DisplayName = "Double" },
                        new PickerItem { Id = (Int32)ArrayTypes.BOOL, DisplayName = "Bool" }/*,
                        new PickerItem { Id = (Int32)ArrayTypes.RGB, DisplayName = "RGB" },
                        new PickerItem { Id = (Int32)ArrayTypes.RGBA, DisplayName = "RGBA" },
                        new PickerItem { Id = (Int32)ArrayTypes.RGBF, DisplayName = "RGBF" },
                        new PickerItem { Id = (Int32)ArrayTypes.RGBAF, DisplayName = "RGBAF" }*/
                    };
                } break;
                case (Int32)PrimaryTypes.COLOR:
                {
                    itemList = new List<PickerItem>
                    {
                        new PickerItem { Id = (Int32)ColorTypes.RGB, DisplayName = "RGB" },
                        new PickerItem { Id = (Int32)ColorTypes.RGBA, DisplayName = "RGBA" },
                        new PickerItem { Id = (Int32)ColorTypes.RGBF, DisplayName = "RGBF" },
                        new PickerItem { Id = (Int32)ColorTypes.RGBAF, DisplayName = "RGBAF" }
                    };
                } break;
                case (Int32)PrimaryTypes.STRING:
                {
                    itemList = new List<PickerItem>
                    {
                        new PickerItem { Id = (Int32)StringTypes.UTF8, DisplayName = "UTF-8" },
                        new PickerItem { Id = (Int32)StringTypes.UTF16LE, DisplayName = "UTF-16 LE" },
                        new PickerItem { Id = (Int32)StringTypes.UTF16BE, DisplayName = "UTF-16 BE" },
                        new PickerItem { Id = (Int32)StringTypes.UTF32LE, DisplayName = "UTF-32 LE" },
                        new PickerItem { Id = (Int32)StringTypes.UTF32BE, DisplayName = "UTF-32 BE" },
                        new PickerItem { Id = (Int32)StringTypes.ASCII, DisplayName = "ASCII" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_1, DisplayName = "ISO-8859-1" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_2, DisplayName = "ISO-8859-2" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_3, DisplayName = "ISO-8859-3" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_4, DisplayName = "ISO-8859-4" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_5, DisplayName = "ISO-8859-5" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_6, DisplayName = "ISO-8859-6" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_7, DisplayName = "ISO-8859-7" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_8, DisplayName = "ISO-8859-8" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_9, DisplayName = "ISO-8859-9" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_10, DisplayName = "ISO-8859-10" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_11, DisplayName = "ISO-8859-11" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_13, DisplayName = "ISO-8859-13" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_14, DisplayName = "ISO-8859-14" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_15, DisplayName = "ISO-8859-15" },
                        new PickerItem { Id = (Int32)StringTypes.ISO_8859_16, DisplayName = "ISO-8859-16" },
                        new PickerItem { Id = (Int32)StringTypes.SHIFTJIS_CP932, DisplayName = "Shift-JIS CP932" },
                        new PickerItem { Id = (Int32)StringTypes.JIS_X_0201_FULLWIDTH, DisplayName = "JIS X 0201 Fullwidth" },
                        new PickerItem { Id = (Int32)StringTypes.JIS_X_0201_HALFWIDTH, DisplayName = "JIS X 0201 Halfwidth" },
                        new PickerItem { Id = (Int32)StringTypes.KS_X_1001, DisplayName = "KS X 1001" },
                        new PickerItem { Id = (Int32)StringTypes.POKEMON_GEN1_ENGLISH, DisplayName = "Pokémon Gen 1 English" },
                        new PickerItem { Id = (Int32)StringTypes.POKEMON_GEN1_FRENCH_GERMAN, DisplayName = "Pokémon Gen 1 French German" },
                        new PickerItem { Id = (Int32)StringTypes.POKEMON_GEN1_ITALIAN_SPANISH, DisplayName = "Pokémon Gen 1 Italian Spanish" },
                        new PickerItem { Id = (Int32)StringTypes.POKEMON_GEN1_JAPANESE, DisplayName = "Pokémon Gen 1 Japanese" },
                        new PickerItem { Id = (Int32)StringTypes.POKEMON_GEN2_ENGLISH, DisplayName = "Pokémon Gen 2 English" }
                    };
                } break;
                default: //PRIMITIVE
                {
                    itemList = new List<PickerItem>
                    {
                        new PickerItem { Id = (Int32)PrimitiveTypes.SINT8, DisplayName = "Int 8" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.UINT8, DisplayName = "UInt 8" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.SINT16, DisplayName = "Int 16" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.UINT16, DisplayName = "UInt 16" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.SINT32, DisplayName = "Int 32" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.UINT32, DisplayName = "UInt 32" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.SINT64, DisplayName = "Int 64" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.UINT64, DisplayName = "UInt 64" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.FLOAT, DisplayName = "Float" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.DOUBLE, DisplayName = "Double" },
                        new PickerItem { Id = (Int32)PrimitiveTypes.BOOL, DisplayName = "Bool" }
                    };
                } break;
            }

            SecondaryTypePicker.ItemsSource = itemList;
            SecondaryTypePicker.ItemDisplayBinding = new Binding("DisplayName");
            SecondaryTypePicker.SelectedIndex = 0;
        }
    };
}