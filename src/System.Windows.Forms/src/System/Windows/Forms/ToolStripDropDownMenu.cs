﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
    [Designer("System.Windows.Forms.Design.ToolStripDropDownDesigner, " + AssemblyRef.SystemDesign)]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class ToolStripDropDownMenu : ToolStripDropDown
    {
            private static readonly Padding ImagePadding    = new Padding(2);
            private static readonly Padding TextPadding     = new Padding(8,1,9,1);
            private static readonly Padding CheckPadding    = new Padding(5,2,2,2);
            private static readonly Padding ArrowPadding    = new Padding(0,0,8,0);

            // This is totally a UI Fudge - if we have an image or check margin with
            // no image or checks in it use this - which is consistent with office
            // and an image margin with a 16x16 icon in it.
            private static int DefaultImageMarginWidth = 24; // 24+1px border - with scaling we add this 1px to new, scaled, field value
            private static int DefaultImageAndCheckMarginWidth = 46;  // 46+1px border - with scaling we add this 1px to new, scaled, field value

            private static int ArrowSize = 10;

            private Size maxItemSize            = Size.Empty;
            private Rectangle checkRectangle    = Rectangle.Empty;
            private Rectangle imageRectangle    = Rectangle.Empty;
            private Rectangle arrowRectangle    = Rectangle.Empty;
            private Rectangle textRectangle     = Rectangle.Empty;
            private Rectangle imageMarginBounds = Rectangle.Empty;
            private int paddingToTrim = 0;
            private int tabWidth = -1;

            private ToolStripScrollButton upScrollButton = null;
            private ToolStripScrollButton downScrollButton = null;
            private int scrollAmount = 0;
            private int indexOfFirstDisplayedItem = -1;

            private BitVector32                    state                               = new BitVector32();

            private static readonly int stateShowImageMargin          = BitVector32.CreateMask();
            private static readonly int stateShowCheckMargin          = BitVector32.CreateMask(stateShowImageMargin);
            private static readonly int stateMaxItemSizeValid         = BitVector32.CreateMask(stateShowCheckMargin);

            private static readonly Size DefaultImageSize = new Size(16, 16);

            private Size scaledDefaultImageSize                = DefaultImageSize;
            private int scaledDefaultImageMarginWidth          = DefaultImageMarginWidth + 1; // 1px for border
            private int scaledDefaultImageAndCheckMarginWidth  = DefaultImageAndCheckMarginWidth + 1; // 1px for border
            private Padding scaledImagePadding                 = ImagePadding;
            private Padding scaledTextPadding                  = TextPadding;
            private Padding scaledCheckPadding                 = CheckPadding;
            private Padding scaledArrowPadding                 = ArrowPadding;
            private int scaledArrowSize                        = ArrowSize;

            /// <devdoc>
            /// Summary of ToolStripDropDown.
            /// </devdoc>
            public ToolStripDropDownMenu(){
            }

            /// <devdoc>
            /// Constructor to autogenerate
            /// </devdoc>
            internal ToolStripDropDownMenu(ToolStripItem ownerItem, bool isAutoGenerated) : base(ownerItem,isAutoGenerated) {
                if (DpiHelper.IsScalingRequired) {
                    scaledDefaultImageSize = DpiHelper.LogicalToDeviceUnits(DefaultImageSize);
                    scaledDefaultImageMarginWidth = DpiHelper.LogicalToDeviceUnitsX(DefaultImageMarginWidth) + 1; // 1px for border
                    scaledDefaultImageAndCheckMarginWidth = DpiHelper.LogicalToDeviceUnitsX(DefaultImageAndCheckMarginWidth) + 1; // 1px for border
                    scaledImagePadding = DpiHelper.LogicalToDeviceUnits(ImagePadding);
                    scaledTextPadding = DpiHelper.LogicalToDeviceUnits(TextPadding);
                    scaledCheckPadding = DpiHelper.LogicalToDeviceUnits(CheckPadding);
                    scaledArrowPadding = DpiHelper.LogicalToDeviceUnits(ArrowPadding);
                    scaledArrowSize = DpiHelper.LogicalToDeviceUnitsX(ArrowSize);
                }
            }

            internal override bool AllItemsVisible {
                get {
                    return !RequiresScrollButtons;
                }
                set {
                    RequiresScrollButtons = !value;
                }
            }

            internal Rectangle ArrowRectangle {
                get {
                    return arrowRectangle;
                }
            }

            internal Rectangle CheckRectangle {
                get {
                    return checkRectangle;
                }
            }

            protected override Padding DefaultPadding {
                get {
                    RightToLeft rightToLeft = RightToLeft;

                    int textPadding = (rightToLeft == RightToLeft.Yes) ? scaledTextPadding.Right : scaledTextPadding.Left;
                    int padding = (ShowCheckMargin || ShowImageMargin) ? textPadding + ImageMargin.Width : textPadding;

                    // scooch in all the items by the margin.
                    if (rightToLeft == RightToLeft.Yes) {
                         return new Padding(1,2,padding,2);
                    }
                    return new Padding(padding,2,1,2);
                }
            }

            public override Rectangle DisplayRectangle {
                get {
                    Rectangle rect = base.DisplayRectangle;
                    if (GetToolStripState(STATE_SCROLLBUTTONS)) {

                        rect.Y += UpScrollButton.Height + UpScrollButton.Margin.Vertical;
                        rect.Height -= UpScrollButton.Height + UpScrollButton.Margin.Vertical + DownScrollButton.Height + DownScrollButton.Margin.Vertical;
                        // Because we're going to draw the scroll buttons on top of the padding, we need to add it back in here.
                        rect = LayoutUtils.InflateRect(rect, new Padding(0, this.Padding.Top, 0, this.Padding.Bottom));
                    }
                    return rect;
                }
            }

            private ToolStripScrollButton DownScrollButton {
                get {
                    if (downScrollButton == null) {
                        downScrollButton = new ToolStripScrollButton(false);
                        downScrollButton.ParentInternal = this;
                    }
                    return downScrollButton;
                }
            }
            /// <devdoc>
            ///  the rectangle representing
            /// </devdoc>
            internal Rectangle ImageRectangle {
                get {
                    return imageRectangle;
                }
            }

            internal int PaddingToTrim {
                get {
                    return paddingToTrim;
                }
                set {
                    if (paddingToTrim != value) {
                        paddingToTrim = value;
                        AdjustSize();
                    }

                }
            }

            /// <devdoc>
            ///  the rectangle representing the color stripe in the menu - this will appear as AffectedBounds
            ///  in the ToolStripRenderEventArgs
            /// </devdoc>
            internal Rectangle ImageMargin {
                get {
                    imageMarginBounds.Height =this.Height;
                    return imageMarginBounds;
                }
            }

            public override LayoutEngine LayoutEngine {
                get {
                     return ToolStripDropDownLayoutEngine.LayoutInstance;
                }
            }

            [
            DefaultValue(ToolStripLayoutStyle.Flow)
            ]
            public new ToolStripLayoutStyle LayoutStyle {
                get { return base.LayoutStyle; }
                set { base.LayoutStyle = value; }
            }

            protected internal override Size MaxItemSize {
                get {
                    if (!state[stateMaxItemSizeValid]) {
                        CalculateInternalLayoutMetrics();
                    }
                    return maxItemSize;
                }
            }

            [
            DefaultValue(true),
            SRDescription(nameof(SR.ToolStripDropDownMenuShowImageMarginDescr)),
            SRCategory(nameof(SR.CatAppearance))
            ]
            public bool ShowImageMargin {
                get {
                    return state[stateShowImageMargin];
                }
                set {
                    if (value != state[stateShowImageMargin]) {
                        state[stateShowImageMargin] = value;
                        LayoutTransaction.DoLayout(this, this, PropertyNames.ShowImageMargin);
                    }
                }
            }

            [
            DefaultValue(false),
            SRDescription(nameof(SR.ToolStripDropDownMenuShowCheckMarginDescr)),
            SRCategory(nameof(SR.CatAppearance))
            ]
            public bool ShowCheckMargin {
                get {
                    return state[stateShowCheckMargin];
                }
                set {
                    if (value != state[stateShowCheckMargin]) {
                        state[stateShowCheckMargin] = value;
                        LayoutTransaction.DoLayout(this, this, PropertyNames.ShowCheckMargin);
                    }
                }
            }


            internal Rectangle TextRectangle {
                get {
                    return textRectangle;
                }
            }

            private ToolStripScrollButton UpScrollButton {
                get {
                    if (upScrollButton == null) {
                        upScrollButton = new ToolStripScrollButton(true);
                        upScrollButton.ParentInternal = this;
                    }
                    return upScrollButton;
                }
            }

            /// <devdoc> this takes a native menu and builds up a managed toolstrip around it.
            ///          Scenario: showing the items from the SystemMenu.
            ///          targetWindow is the window to send WM_COMMAND, WM_SYSCOMMAND to
            ///          hmenu is a handle to the native menu
            ///
            ///

            internal static ToolStripDropDownMenu FromHMenu(IntPtr hmenu, IWin32Window targetWindow) {
                ToolStripDropDownMenu managedDropDown = new ToolStripDropDownMenu();
                managedDropDown.SuspendLayout();


                HandleRef menuHandle = new HandleRef(null, hmenu);
                int count = UnsafeNativeMethods.GetMenuItemCount(menuHandle);

                ToolStripItem itemToAdd;

                // surf through the items in the collection, building up TSMenuItems and TSSeparators
                // corresponding to the native menu.
                for (int i = 0; i < count; i++) {
                    // peek at the i'th item.
                    NativeMethods.MENUITEMINFO_T_RW info = new NativeMethods.MENUITEMINFO_T_RW();
                    info.cbSize = Marshal.SizeOf<NativeMethods.MENUITEMINFO_T_RW>();
                    info.fMask = NativeMethods.MIIM_FTYPE;
                    info.fType = NativeMethods.MIIM_FTYPE;
                    UnsafeNativeMethods.GetMenuItemInfo(menuHandle, i, /*fByPosition=*/ true, info);

                    if (info.fType == NativeMethods.MFT_SEPARATOR){
                        // its a separator.
                    	itemToAdd = new ToolStripSeparator();
                    }
                    else {
                        // its a menu item... lets fish out the command id
                     	info = new NativeMethods.MENUITEMINFO_T_RW();
                    	info.cbSize = Marshal.SizeOf<NativeMethods.MENUITEMINFO_T_RW>();
                    	info.fMask = NativeMethods.MIIM_ID;
                    	info.fType = NativeMethods.MIIM_ID;
                    	UnsafeNativeMethods.GetMenuItemInfo(menuHandle, i, /*fByPosition=*/ true, info);

                        // create the managed object - toolstripmenu item knows how to grok hmenu for information.
                    	itemToAdd = new ToolStripMenuItem(hmenu, info.wID, targetWindow);


                    	// if there is a submenu fetch it.
                    	info = new NativeMethods.MENUITEMINFO_T_RW();
                    	info.cbSize = Marshal.SizeOf<NativeMethods.MENUITEMINFO_T_RW>();
                    	info.fMask = NativeMethods.MIIM_SUBMENU;
                    	info.fType = NativeMethods.MIIM_SUBMENU;
                    	UnsafeNativeMethods.GetMenuItemInfo(menuHandle, i, /*fByPosition=*/ true, info);

                    	if (info.hSubMenu != IntPtr.Zero) {
                    	    // set the dropdown to be the items from the submenu
                    		((ToolStripMenuItem)itemToAdd).DropDown = FromHMenu(info.hSubMenu, targetWindow);
                    	}
                    }

                    managedDropDown.Items.Add(itemToAdd);
                }
                managedDropDown.ResumeLayout();
                return managedDropDown;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")] // using "\t" to figure out the width of tab
            private void  CalculateInternalLayoutMetrics() {


                Size maxTextSize    = Size.Empty;
                Size maxImageSize   = Size.Empty;
                Size maxCheckSize   = scaledDefaultImageSize;
                Size maxArrowSize   = Size.Empty;
                Size maxNonMenuItemSize = Size.Empty;

                // determine Text Metrics
                for (int i = 0; i < Items.Count; i++)  {
                    ToolStripItem item = Items[i];
                    ToolStripMenuItem menuItem = item as ToolStripMenuItem;

                    if (menuItem != null) {

                        Size menuItemTextSize = menuItem.GetTextSize();

                        if (menuItem.ShowShortcutKeys)  {
                            Size shortcutTextSize = menuItem.GetShortcutTextSize();
                            if (tabWidth == -1) {
                                tabWidth = TextRenderer.MeasureText("\t", this.Font).Width;
                            }
                            menuItemTextSize.Width += tabWidth + shortcutTextSize.Width;
                            menuItemTextSize.Height = Math.Max(menuItemTextSize.Height, shortcutTextSize.Height);
                        }

                        // we truly only care about the maximum size we find.
                        maxTextSize.Width = Math.Max(maxTextSize.Width, menuItemTextSize.Width);
                        maxTextSize.Height = Math.Max(maxTextSize.Height, menuItemTextSize.Height);

                        // determine Image Metrics
                        Size imageSize = Size.Empty;
                        if (menuItem.Image != null) {
                            imageSize = (menuItem.ImageScaling == ToolStripItemImageScaling.SizeToFit) ? ImageScalingSize : menuItem.Image.Size;
                        }

                        maxImageSize.Width = Math.Max(maxImageSize.Width, imageSize.Width);
                        maxImageSize.Height = Math.Max(maxImageSize.Height, imageSize.Height);

                        if (menuItem.CheckedImage != null) {
                            Size checkedImageSize = menuItem.CheckedImage.Size;
                            maxCheckSize.Width = Math.Max(checkedImageSize.Width, maxCheckSize.Width);
                            maxCheckSize.Height = Math.Max(checkedImageSize.Height, maxCheckSize.Height);
                        }
                    }
                    else if (!(item is ToolStripSeparator)) {
                        maxNonMenuItemSize.Height = Math.Max(item.Bounds.Height, maxNonMenuItemSize.Height);
                        maxNonMenuItemSize.Width = Math.Max(item.Bounds.Width, maxNonMenuItemSize.Width);
                    }
                }

                this.maxItemSize.Height =  Math.Max(maxTextSize.Height + scaledTextPadding.Vertical, Math.Max(maxCheckSize.Height + scaledCheckPadding.Vertical, maxArrowSize.Height + scaledArrowPadding.Vertical));

                if (ShowImageMargin) {
                    // only add in the image into the calculation if we're going to render it.
                    this.maxItemSize.Height = Math.Max(maxImageSize.Height + scaledImagePadding.Vertical, maxItemSize.Height);
                }

                bool  useDefaultCheckMarginWidth = (ShowCheckMargin && (maxCheckSize.Width == 0));
                bool  useDefaultImageMarginWidth = (ShowImageMargin && (maxImageSize.Width == 0));
                // Always save space for an arrow
                maxArrowSize = new Size(scaledArrowSize, maxItemSize.Height);

                maxTextSize.Height = maxItemSize.Height -  scaledTextPadding.Vertical;
                maxImageSize.Height = maxItemSize.Height - scaledImagePadding.Vertical;
                maxCheckSize.Height = maxItemSize.Height - scaledCheckPadding.Vertical;

               // fixup if there are non-menu items that are larger than our normal menu items
               maxTextSize.Width = Math.Max(maxTextSize.Width, maxNonMenuItemSize.Width);

               Point nextPoint = Point.Empty;
               int checkAndImageMarginWidth = 0;

               int extraImageWidth = Math.Max(0, maxImageSize.Width - scaledDefaultImageSize.Width);

               if (ShowCheckMargin && ShowImageMargin) {
                    // double column - check margin then image margin
                    // default to 46px - grow if necessary.
                    checkAndImageMarginWidth = scaledDefaultImageAndCheckMarginWidth;

                    // add in the extra space for the image... since the check size is locked down to 16x16.
                    checkAndImageMarginWidth += extraImageWidth;

                    // align the checkmark
                    nextPoint = new Point(scaledCheckPadding.Left, scaledCheckPadding.Top);
               		checkRectangle = LayoutUtils.Align(maxCheckSize, new Rectangle(nextPoint.X, nextPoint.Y, maxCheckSize.Width, maxItemSize.Height), ContentAlignment.MiddleCenter);

                    // align the image rectangle
                    nextPoint.X = checkRectangle.Right + scaledCheckPadding.Right + scaledImagePadding.Left;
                    nextPoint.Y = scaledImagePadding.Top;
                    imageRectangle = LayoutUtils.Align(maxImageSize, new Rectangle(nextPoint.X, nextPoint.Y, maxImageSize.Width, maxItemSize.Height), ContentAlignment.MiddleCenter);

               }
               else if (ShowCheckMargin) {
                   // no images should be shown in a ShowCheckMargin only scenario.
                   // default to 24px - grow if necessary.
                   checkAndImageMarginWidth = scaledDefaultImageMarginWidth;

                   // align the checkmark
                    nextPoint = new Point(1,scaledCheckPadding.Top);
               //    nextPoint = new Point(scaledCheckPadding.Left, scaledCheckPadding.Top);
                   checkRectangle = LayoutUtils.Align(maxCheckSize, new Rectangle(nextPoint.X, nextPoint.Y, checkAndImageMarginWidth, maxItemSize.Height), ContentAlignment.MiddleCenter);

                   imageRectangle = Rectangle.Empty;

               }
               else if (ShowImageMargin) {
                   // checks and images render in the same area.

                   // default to 24px - grow if necessary.
                   checkAndImageMarginWidth = scaledDefaultImageMarginWidth;

                   // add in the extra space for the image... since the check size is locked down to 16x16.
                   checkAndImageMarginWidth += extraImageWidth;

                   // NOTE due to the Padding property, we're going to have to recalc the vertical alignment in ToolStripMenuItemInternalLayout.
                   // Dont fuss here over the Y, X is what's critical.

                   // check and image rect are the same - take the max of the image size and the check size and align
                   nextPoint = new Point(1, scaledCheckPadding.Top);
                   checkRectangle = LayoutUtils.Align(LayoutUtils.UnionSizes(maxCheckSize,maxImageSize), new Rectangle(nextPoint.X, nextPoint.Y, checkAndImageMarginWidth-1, maxItemSize.Height), ContentAlignment.MiddleCenter);

                   // align the image
                   imageRectangle = checkRectangle;

               }
               else {
                    checkAndImageMarginWidth = 0;
               }
               nextPoint.X = checkAndImageMarginWidth+1;


                // calculate space for image
                // if we didnt have a check - make sure to ignore check padding

                // consider: should we constrain to a reasonable width?
                //imageMarginBounds = new Rectangle(0, 0, Math.Max(imageMarginWidth,DefaultImageMarginWidth), this.Height);
                imageMarginBounds = new Rectangle(0,0,checkAndImageMarginWidth, this.Height);

                // calculate space for shortcut and text
                nextPoint.X = imageMarginBounds.Right+ scaledTextPadding.Left;
                nextPoint.Y = scaledTextPadding.Top;
                textRectangle = new Rectangle(nextPoint, maxTextSize);

                // calculate space for arrow
                nextPoint.X = textRectangle.Right+ scaledTextPadding.Right + scaledArrowPadding.Left;
                nextPoint.Y = scaledArrowPadding.Top;
                arrowRectangle = new Rectangle(nextPoint, maxArrowSize);

                // calculate space required for all of these pieces
                this.maxItemSize.Width =  (arrowRectangle.Right + scaledArrowPadding.Right) - imageMarginBounds.Left;

                this.Padding = DefaultPadding;
                int trimPadding = imageMarginBounds.Width;

                if (RightToLeft == RightToLeft.Yes) {
                    // reverse the rectangle alignment in RightToLeft.Yes
                    trimPadding += scaledTextPadding.Right;
                    int width = maxItemSize.Width;
                    checkRectangle.X        = width - checkRectangle.Right;
                    imageRectangle.X        = width - imageRectangle.Right;
                    textRectangle.X         = width - textRectangle.Right;
                    arrowRectangle.X        = width - arrowRectangle.Right;
                    imageMarginBounds.X     = width - imageMarginBounds.Right;

                }
                else {
                    trimPadding += scaledTextPadding.Left;
                }



                // We need to make sure that the text really appears vertically centered - this can be a problem in
                // systems which force the text rectangle to be odd.

                // force this to be an even height.
                this.maxItemSize.Height += this.maxItemSize.Height%2;

                textRectangle.Y = LayoutUtils.VAlign(textRectangle.Size, new Rectangle(Point.Empty, maxItemSize), ContentAlignment.MiddleCenter).Y;
                textRectangle.Y += (textRectangle.Height %2); // if the height is odd, push down by one px
                state[stateMaxItemSizeValid]=true;
                this.PaddingToTrim = trimPadding;

            }

            internal override void ChangeSelection(ToolStripItem nextItem) {
                if (nextItem != null) {
                    Rectangle displayRect = DisplayRectangle;
                    if (!displayRect.Contains(displayRect.X, nextItem.Bounds.Top)
                        || !displayRect.Contains(displayRect.X, nextItem.Bounds.Bottom)) {

                        int delta;
                        if (displayRect.Y > nextItem.Bounds.Top) {
                            delta = nextItem.Bounds.Top - displayRect.Y;
                        }
                        else {
                            delta = nextItem.Bounds.Bottom - (displayRect.Y + displayRect.Height);
                            // Now adjust so that the item at the top isn't truncated.
                            int index = this.Items.IndexOf(nextItem);
                            while (index >= 0) {
                                // we need to roll back to the index which is visible
                                if ( (this.Items[index].Visible && displayRect.Contains(displayRect.X, this.Items[index].Bounds.Top - delta))
                                    || !this.Items[index].Visible) {
                                    --index;
                                }
                                else {
                                    break;
                                }
                            }

                            if (index >= 0) {
                                if (displayRect.Contains(displayRect.X, this.Items[index].Bounds.Bottom - delta)) {
                                    // We found an item which is truncated at the top.
                                    delta += (this.Items[index].Bounds.Bottom - delta) - displayRect.Top;
                                }
                            }
                        }
                        this.ScrollInternal(delta);
                        this.UpdateScrollButtonStatus();
                    }
                }
                base.ChangeSelection(nextItem);
            }

            protected internal override ToolStripItem CreateDefaultItem(string text, Image image, EventHandler onClick) {
                if (text == "-") {
                    return new ToolStripSeparator();
                }
                else {
                    return new ToolStripMenuItem(text,image,onClick);
                }
            }

            internal override ToolStripItem GetNextItem(ToolStripItem start, ArrowDirection direction, bool rtlAware) {
                  // for up/down we dont care about flipping left/right tab should still take you down.
                return GetNextItem(start, direction);
            }

            internal override void Initialize() {
                base.Initialize();
                this.Padding = DefaultPadding;
                FlowLayoutSettings settings = FlowLayout.CreateSettings(this);
                settings.FlowDirection = FlowDirection.TopDown;
                state[stateShowImageMargin] = true;
            }
            protected override void OnLayout(LayoutEventArgs e) {
                if (!this.IsDisposed)
                {
                    // We always layout as if we don't need scroll buttons.
                    // If we do, then we'll adjust the positions to match.
                    this.RequiresScrollButtons = false;
                    CalculateInternalLayoutMetrics();
                    base.OnLayout(e);
                    if (!this.RequiresScrollButtons) {
                        this.ResetScrollPosition();
                    }
                }
            }

            protected override void OnFontChanged(EventArgs e) {
                tabWidth = -1;
                base.OnFontChanged(e);

            }
            protected override void OnPaintBackground(PaintEventArgs e) {
                base.OnPaintBackground(e);
                if (ShowCheckMargin || ShowImageMargin) {
                    Renderer.DrawImageMargin(new ToolStripRenderEventArgs(e.Graphics, this, this.ImageMargin, SystemColors.Control));
                }
            }

            internal override bool RequiresScrollButtons {
                get {
                    return GetToolStripState(STATE_SCROLLBUTTONS);
                }
                set {
                    bool changed = (RequiresScrollButtons != value);
                    SetToolStripState(STATE_SCROLLBUTTONS, value);
                    if (changed) {
                        UpdateScrollButtonLocations();
                        if (this.Items.Count > 0) {
                            int delta = this.Items[0].Bounds.Top - this.DisplayRectangle.Top;
                            this.ScrollInternal(delta);
                            this.scrollAmount -= delta;
                            if (value) {
                                RestoreScrollPosition();
                            }
                        }
                        else {
                            this.scrollAmount = 0;
                        }
                    }
                }
            }

            internal void ResetScrollPosition() {
                this.scrollAmount = 0;
            }

            internal void RestoreScrollPosition() {
                if (!RequiresScrollButtons || this.Items.Count == 0) {
                    return;
                }

                // We don't just scroll by the amount, because that might
                // cause the bottom of the menu to be blank if some items have
                // been removed/hidden since the last time we were displayed.
                // This also deals with items of different height, so that we don't truncate
                // and items under the top scrollbar.

                Rectangle displayRectangle = this.DisplayRectangle;
                int alreadyScrolled = displayRectangle.Top - this.Items[0].Bounds.Top;

                int requiredScrollAmount = this.scrollAmount - alreadyScrolled;

                int deltaToScroll = 0;
                if (requiredScrollAmount > 0) {
                    for (int i = 0; i < this.Items.Count && deltaToScroll < requiredScrollAmount; ++i) {
                        if (this.Items[i].Available) {
                            Rectangle adjustedLastItemBounds = this.Items[this.Items.Count - 1].Bounds;
                            adjustedLastItemBounds.Y -= deltaToScroll;
                            if (displayRectangle.Contains(displayRectangle.X, adjustedLastItemBounds.Top)
                                && displayRectangle.Contains(displayRectangle.X, adjustedLastItemBounds.Bottom)) {
                                // Scrolling this amount would make the last item visible, so don't scroll any more.
                                break;
                            }

                            // We use a delta between the tops, since it takes margin's and padding into account.
                            if (i < this.Items.Count - 1) {
                                deltaToScroll += this.Items[i + 1].Bounds.Top - this.Items[i].Bounds.Top;
                            }
                            else {
                                deltaToScroll += this.Items[i].Bounds.Height;
                            }
                        }
                    }
                }
                else {
                    for (int i = this.Items.Count - 1; i >= 0 && deltaToScroll > requiredScrollAmount; --i) {
                        if (this.Items[i].Available) {
                            Rectangle adjustedLastItemBounds = this.Items[0].Bounds;
                            adjustedLastItemBounds.Y -= deltaToScroll;
                            if (displayRectangle.Contains(displayRectangle.X, adjustedLastItemBounds.Top)
                                && displayRectangle.Contains(displayRectangle.X, adjustedLastItemBounds.Bottom)) {
                                // Scrolling this amount would make the last item visible, so don't scroll any more.
                                break;
                            }

                            // We use a delta between the tops, since it takes margin's and padding into account.
                            if (i > 0) {
                                deltaToScroll -= this.Items[i].Bounds.Top - this.Items[i - 1].Bounds.Top;
                            }
                            else {
                                deltaToScroll -= this.Items[i].Bounds.Height;
                            }
                        }
                    }
                }
                this.ScrollInternal(deltaToScroll);
                this.scrollAmount = this.DisplayRectangle.Top - this.Items[0].Bounds.Top;
                UpdateScrollButtonLocations();
            }


            internal override void ScrollInternal(int delta) {
                base.ScrollInternal(delta);
                this.scrollAmount += delta;
            }

            internal void ScrollInternal(bool up) {
                UpdateScrollButtonStatus();

                // calling this to get ScrollWindowEx.  In actuality it does nothing
                // to change the display rect!
                int delta;
                if (this.indexOfFirstDisplayedItem == -1 || this.indexOfFirstDisplayedItem >= this.Items.Count) {
                    Debug.Fail("Why wasn't 'UpdateScrollButtonStatus called'? We don't have the item to scroll by");
                    int menuHeight = SystemInformation.MenuHeight;

                    delta = up ? -menuHeight : menuHeight;
                }
                else {
                    if (up) {
                        if (this.indexOfFirstDisplayedItem == 0) {
                            Debug.Fail("We're trying to scroll up, but the top item is displayed!!!");
                            delta = 0;
                        }
                        else {
                            ToolStripItem itemTop = this.Items[this.indexOfFirstDisplayedItem - 1];
                            ToolStripItem itemBottom = this.Items[this.indexOfFirstDisplayedItem];
                            // We use a delta between the tops, since it takes margin's and padding into account.
                            delta = itemTop.Bounds.Top - itemBottom.Bounds.Top;
                        }
                    }
                    else {
                        if (this.indexOfFirstDisplayedItem == this.Items.Count - 1) {
                            Debug.Fail("We're trying to scroll down, but the top item is displayed!!!");
                            delta = 0;
                        }

                        ToolStripItem itemTop = this.Items[this.indexOfFirstDisplayedItem];
                        ToolStripItem itemBottom = this.Items[this.indexOfFirstDisplayedItem + 1];
                        // We use a delta between the tops, since it takes margin's and padding into account.
                        delta = itemBottom.Bounds.Top - itemTop.Bounds.Top;
                    }
                }
                ScrollInternal(delta);
                UpdateScrollButtonLocations();
            }

            protected override void SetDisplayedItems() {

                base.SetDisplayedItems();
                if (RequiresScrollButtons) {

                    this.DisplayedItems.Add(UpScrollButton);
                    this.DisplayedItems.Add(DownScrollButton);
                    UpdateScrollButtonLocations();
                    UpScrollButton.Visible = true;
                    DownScrollButton.Visible = true;
                }
                else {
                    UpScrollButton.Visible = false;
                    DownScrollButton.Visible = false;
                }
            }

            private void UpdateScrollButtonLocations() {

                if (GetToolStripState(STATE_SCROLLBUTTONS)) {
                    Size upSize = UpScrollButton.GetPreferredSize(Size.Empty);
                    //
                    Point upLocation = new Point(1, 0);

                    UpScrollButton.SetBounds(new Rectangle(upLocation, upSize));

                    Size downSize = DownScrollButton.GetPreferredSize(Size.Empty);
                    int height = GetDropDownBounds(this.Bounds).Height;

                    Point downLocation = new Point(1, height - downSize.Height);
                    DownScrollButton.SetBounds(new Rectangle(downLocation, downSize));
                    UpdateScrollButtonStatus();
                }

            }

            private void UpdateScrollButtonStatus() {
                Rectangle displayRectangle = this.DisplayRectangle;

                this.indexOfFirstDisplayedItem = -1;
                int minY = int.MaxValue, maxY = 0;

                for (int i = 0; i < this.Items.Count; ++i) {
                    ToolStripItem item = this.Items[i];
                    if (UpScrollButton == item) {
                        continue;
                    }
                    if (DownScrollButton == item) {
                        continue;
                    }

                    if (!item.Available) {
                        continue;
                    }

                    if (this.indexOfFirstDisplayedItem == -1 && displayRectangle.Contains(displayRectangle.X, item.Bounds.Top)) {
                        this.indexOfFirstDisplayedItem = i;
                    }

                    minY = Math.Min(minY, item.Bounds.Top);
                    maxY = Math.Max(maxY, item.Bounds.Bottom);
                }

                UpScrollButton.Enabled = !displayRectangle.Contains(displayRectangle.X, minY);
                DownScrollButton.Enabled = !displayRectangle.Contains(displayRectangle.X, maxY);
            }

            internal sealed class ToolStripDropDownLayoutEngine : FlowLayout {

                public static ToolStripDropDownLayoutEngine LayoutInstance = new ToolStripDropDownLayoutEngine();

                internal override Size GetPreferredSize(IArrangedElement container, Size proposedConstraints) {
                        Size preferredSize = base.GetPreferredSize(container, proposedConstraints);
                        ToolStripDropDownMenu dropDownMenu = container as ToolStripDropDownMenu;
                        if (dropDownMenu != null) {
                            preferredSize.Width = dropDownMenu.MaxItemSize.Width - dropDownMenu.PaddingToTrim;
                        }
                        return preferredSize;
                }

           }
       }
    }
