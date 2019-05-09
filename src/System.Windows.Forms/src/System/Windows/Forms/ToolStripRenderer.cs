﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms {

    using System.Windows.Forms.VisualStyles;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms.Internal;
    using System.Drawing.Imaging;
    using System.ComponentModel;
    using System.Windows.Forms.Layout;

    public abstract class ToolStripRenderer {

        private static readonly object EventRenderSplitButtonBackground             = new object();
        private static readonly object EventRenderItemBackground                    = new object();
        private static readonly object EventRenderItemImage                         = new object();
        private static readonly object EventRenderItemText                          = new object();
        private static readonly object EventRenderToolStripBackground               = new object();
        private static readonly object EventRenderGrip                              = new object();
        private static readonly object EventRenderButtonBackground                  = new object();
        private static readonly object EventRenderLabelBackground                   = new object();
        private static readonly object EventRenderMenuItemBackground                = new object();
        private static readonly object EventRenderDropDownButtonBackground          = new object();
        private static readonly object EventRenderOverflowButtonBackground          = new object();
        private static readonly object EventRenderImageMargin                       = new object();
        private static readonly object EventRenderBorder                            = new object();
        private static readonly object EventRenderArrow                             = new object();
        private static readonly object EventRenderStatusStripPanelBackground        = new object();
        private static readonly object EventRenderToolStripStatusLabelBackground    = new object();
        private static readonly object EventRenderSeparator                         = new object();
        private static readonly object EventRenderItemCheck                         = new object();
        private static readonly object EventRenderToolStripPanelBackground          = new object();
        private static readonly object EventRenderToolStripContentPanelBackground   = new object();

        private static readonly object EventRenderStatusStripSizingGrip             = new object();

        private static ColorMatrix disabledImageColorMatrix;

        private EventHandlerList events;
        private bool isAutoGenerated = false;

        private static bool isScalingInitialized = false;
        // arrows are rendered as isosceles triangles, whose heights are half the base in order to have 45 degree angles
        // Offset2X is half of the base
        // Offset2Y is height of the isosceles triangle
        private static int OFFSET_2PIXELS = 2;
        private static int OFFSET_4PIXELS = 4;
        protected static int Offset2X = OFFSET_2PIXELS;
        protected static int Offset2Y = OFFSET_2PIXELS;
        private static int offset4X = OFFSET_4PIXELS;
        private static int offset4Y = OFFSET_4PIXELS;

        // this is used in building up the half pyramid of rectangles that are drawn in a
        // status strip sizing grip.
        private static Rectangle[] baseSizeGripRectangles = new Rectangle[] { new Rectangle(8,0,2,2),
                                                                                new Rectangle(8,4,2,2),
                                                                                new Rectangle(8,8,2,2),
                                                                                new Rectangle(4,4,2,2),
                                                                                new Rectangle(4,8,2,2),
                                                                                new Rectangle(0,8,2,2) };

        protected ToolStripRenderer() {
        }

        internal ToolStripRenderer(bool isAutoGenerated) {
            this.isAutoGenerated = isAutoGenerated;
        }
        // this is used in building disabled images.
        private static ColorMatrix DisabledImageColorMatrix {
            get {
                if (disabledImageColorMatrix == null) {
                    // this is the result of a GreyScale matrix multiplied by a transparency matrix of .5



                    float[][] greyscale = new float[5][];
                    greyscale[0] = new float[5] {0.2125f, 0.2125f, 0.2125f, 0, 0};
                    greyscale[1] = new float[5] {0.2577f, 0.2577f, 0.2577f, 0, 0};
                    greyscale[2] = new float[5] {0.0361f, 0.0361f, 0.0361f, 0, 0};
                    greyscale[3] = new float[5] {0,       0,       0,       1, 0};
                    greyscale[4] = new float[5] {0.38f,   0.38f,   0.38f,   0, 1};

                    float[][] transparency = new float[5][];
                    transparency[0] = new float[5] {1, 0,  0,  0,  0};
                    transparency[1] = new float[5] {0, 1,  0,  0,  0};
                    transparency[2] = new float[5] {0, 0,  1,  0,  0};
                    transparency[3] = new float[5] {0, 0,  0, .7F, 0};
                    transparency[4] = new float[5] {0, 0,  0,  0,  0};


                    disabledImageColorMatrix = ControlPaint.MultiplyColorMatrix(transparency, greyscale);

                }

                return disabledImageColorMatrix;
            }
        }


        /// <devdoc>
        ///    <para>Gets the list of event handlers that are attached to this component.</para>
        /// </devdoc>
        private EventHandlerList Events {
            get {
                if (events == null) {
                    events = new EventHandlerList();
                }
                return events;
            }
        }

        internal bool IsAutoGenerated {
            get { return isAutoGenerated; }
        }

        // if we're in a low contrast, high resolution situation, use this renderer under the covers instead.
        internal virtual ToolStripRenderer RendererOverride {
            get {
                return null;
            }
        }

        /// -----------------------------------------------------------------------------
        ///





        public event ToolStripArrowRenderEventHandler RenderArrow {
            add => AddHandler(EventRenderArrow, value);
            remove => RemoveHandler(EventRenderArrow, value);
        }

        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripRenderEventHandler RenderToolStripBackground {
            add => AddHandler(EventRenderToolStripBackground, value);
            remove => RemoveHandler(EventRenderToolStripBackground, value);
        }


        public event ToolStripPanelRenderEventHandler RenderToolStripPanelBackground {
            add => AddHandler(EventRenderToolStripPanelBackground, value);
            remove => RemoveHandler(EventRenderToolStripPanelBackground, value);
        }

        public event ToolStripContentPanelRenderEventHandler RenderToolStripContentPanelBackground {
             add => AddHandler(EventRenderToolStripContentPanelBackground, value);
             remove => RemoveHandler(EventRenderToolStripContentPanelBackground, value);
         }

        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripRenderEventHandler RenderToolStripBorder {
            add => AddHandler(EventRenderBorder, value);
            remove => RemoveHandler(EventRenderBorder, value);
        }

        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderButtonBackground {
            add => AddHandler(EventRenderButtonBackground, value);
            remove => RemoveHandler(EventRenderButtonBackground, value);
        }
        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderDropDownButtonBackground {
            add => AddHandler(EventRenderDropDownButtonBackground, value);
            remove => RemoveHandler(EventRenderDropDownButtonBackground, value);
        }

        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderOverflowButtonBackground {
            add => AddHandler(EventRenderOverflowButtonBackground, value);
            remove => RemoveHandler(EventRenderOverflowButtonBackground, value);
        }
        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripGripRenderEventHandler RenderGrip {
            add => AddHandler(EventRenderGrip, value);
            remove => RemoveHandler(EventRenderGrip, value);
        }
        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderItemBackground {
            add => AddHandler(EventRenderItemBackground, value);
            remove => RemoveHandler(EventRenderItemBackground, value);
        }
        /// <devdoc>
        /// Draws the split button
        /// </devdoc>
        public event ToolStripItemImageRenderEventHandler RenderItemImage {
            add => AddHandler(EventRenderItemImage, value);
            remove => RemoveHandler(EventRenderItemImage, value);
        }
        /// <devdoc>
        /// Draws the checkmark
        /// </devdoc>
        public event ToolStripItemImageRenderEventHandler RenderItemCheck {
            add => AddHandler(EventRenderItemCheck, value);
            remove => RemoveHandler(EventRenderItemCheck, value);
        }
        /// <devdoc>
        /// Draws the split button
        /// </devdoc>
        public event ToolStripItemTextRenderEventHandler RenderItemText {
            add => AddHandler(EventRenderItemText, value);
            remove => RemoveHandler(EventRenderItemText, value);
        }

        public event ToolStripRenderEventHandler RenderImageMargin {
            add => AddHandler(EventRenderImageMargin, value);
            remove => RemoveHandler(EventRenderImageMargin, value);
        }
        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderLabelBackground {
            add => AddHandler(EventRenderLabelBackground, value);
            remove => RemoveHandler(EventRenderLabelBackground, value);
        }
        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderMenuItemBackground {
            add => AddHandler(EventRenderMenuItemBackground, value);
            remove => RemoveHandler(EventRenderMenuItemBackground, value);
        }

        /// <devdoc>
        /// Draws the split button
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderToolStripStatusLabelBackground {
            add => AddHandler(EventRenderToolStripStatusLabelBackground, value);
            remove => RemoveHandler(EventRenderToolStripStatusLabelBackground, value);
        }

        /// <devdoc>
        /// <para>Occurs when the display style has changed</para>
        /// </devdoc>
        public event ToolStripRenderEventHandler RenderStatusStripSizingGrip {
            add => AddHandler(EventRenderStatusStripSizingGrip, value);
            remove => RemoveHandler(EventRenderStatusStripSizingGrip, value);
        }

        /// <devdoc>
        /// Draws the split button
        /// </devdoc>
        public event ToolStripItemRenderEventHandler RenderSplitButtonBackground {
            add => AddHandler(EventRenderSplitButtonBackground, value);
            remove => RemoveHandler(EventRenderSplitButtonBackground, value);
        }


       public event ToolStripSeparatorRenderEventHandler RenderSeparator {
           add => AddHandler(EventRenderSeparator, value);
           remove => RemoveHandler(EventRenderSeparator, value);
       }

#region EventHandlerSecurity
      /// -----------------------------------------------------------------------------
      ///




       private void AddHandler(object key, Delegate value) {
            Events.AddHandler(key, value);
       }

       private void RemoveHandler(object key, Delegate value) {
            Events.RemoveHandler(key, value);
       }
#endregion

       public static Image CreateDisabledImage(Image normalImage) {
            return CreateDisabledImage(normalImage, null);
       }

        public void DrawArrow(ToolStripArrowRenderEventArgs e) {
            OnRenderArrow(e);

            ToolStripArrowRenderEventHandler eh = Events[EventRenderArrow] as ToolStripArrowRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw the background color
        /// </devdoc>
        public void DrawToolStripBackground(ToolStripRenderEventArgs e) {
            OnRenderToolStripBackground(e);

            ToolStripRenderEventHandler eh = Events[EventRenderToolStripBackground] as ToolStripRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }



        /// <devdoc>
        /// Draw the background color
        /// </devdoc>
        public void DrawGrip(ToolStripGripRenderEventArgs e) {
            OnRenderGrip(e);
            ToolStripGripRenderEventHandler eh = Events[EventRenderGrip] as ToolStripGripRenderEventHandler;
            if (eh != null) {
               eh(this, e);
             }
        }


        /// <devdoc>
        /// Draw the item's background.
        /// </devdoc>
        public void DrawItemBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderItemBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderItemBackground] as ToolStripItemRenderEventHandler;
             if (eh != null) {
                 eh(this, e);
             }

        }

        /// <devdoc>
        /// Draw the background color
        /// </devdoc>
        public void DrawImageMargin(ToolStripRenderEventArgs e) {
            OnRenderImageMargin(e);

            ToolStripRenderEventHandler eh = Events[EventRenderImageMargin] as ToolStripRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw the background color
        /// </devdoc>
        public void DrawLabelBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderLabelBackground(e);
            ToolStripItemRenderEventHandler eh = Events[EventRenderLabelBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw the item's background.
        /// </devdoc>
        public void DrawButtonBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderButtonBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderButtonBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        public void DrawToolStripBorder(ToolStripRenderEventArgs e)
        {
           OnRenderToolStripBorder(e);

           ToolStripRenderEventHandler eh = Events[EventRenderBorder] as ToolStripRenderEventHandler;
           if (eh != null) {
               eh(this, e);
           }
        }


        /// <devdoc>
        /// Draw the item's background.
        /// </devdoc>
        public void DrawDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderDropDownButtonBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderDropDownButtonBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw the item's background.
        /// </devdoc>
        public void DrawOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderOverflowButtonBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderOverflowButtonBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw image
        /// </devdoc>
        public void DrawItemImage(ToolStripItemImageRenderEventArgs e) {
            OnRenderItemImage(e);

            ToolStripItemImageRenderEventHandler eh = Events[EventRenderItemImage] as ToolStripItemImageRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw image
        /// </devdoc>
        public void DrawItemCheck(ToolStripItemImageRenderEventArgs e) {
              OnRenderItemCheck(e);

              ToolStripItemImageRenderEventHandler eh = Events[EventRenderItemCheck] as ToolStripItemImageRenderEventHandler;
              if (eh != null) {
                  eh(this, e);
              }
        }

        /// <devdoc>
        /// Draw text
        /// </devdoc>
        public void DrawItemText(ToolStripItemTextRenderEventArgs e) {
            OnRenderItemText(e);

            ToolStripItemTextRenderEventHandler eh = Events[EventRenderItemText] as ToolStripItemTextRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <devdoc>
        /// Draw the item's background.
        /// </devdoc>
        public void DrawMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderMenuItemBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderMenuItemBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }


        /// <devdoc>
        /// Draw the background color
        /// </devdoc>
        public void DrawSplitButton(ToolStripItemRenderEventArgs e) {

            OnRenderSplitButtonBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderSplitButtonBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }

        }

        /// <devdoc>
        /// Draw the background color
        /// </devdoc>
        public void DrawToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e) {

            OnRenderToolStripStatusLabelBackground(e);

            ToolStripItemRenderEventHandler eh = Events[EventRenderToolStripStatusLabelBackground] as ToolStripItemRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }

        }

        //
        public void DrawStatusStripSizingGrip(ToolStripRenderEventArgs e) {

            OnRenderStatusStripSizingGrip(e);

            ToolStripRenderEventHandler eh = Events[EventRenderStatusStripSizingGrip] as ToolStripRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }
        /// <devdoc>
        /// Draw the separator
        /// </devdoc>
        public void DrawSeparator(ToolStripSeparatorRenderEventArgs e) {
            OnRenderSeparator(e);
            ToolStripSeparatorRenderEventHandler eh = Events[EventRenderSeparator] as ToolStripSeparatorRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        public void DrawToolStripPanelBackground(ToolStripPanelRenderEventArgs e) {
            OnRenderToolStripPanelBackground(e);
            ToolStripPanelRenderEventHandler eh = Events[EventRenderToolStripPanelBackground] as ToolStripPanelRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        public void DrawToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e) {
            OnRenderToolStripContentPanelBackground(e);
            ToolStripContentPanelRenderEventHandler eh = Events[EventRenderToolStripContentPanelBackground] as ToolStripContentPanelRenderEventHandler;
            if (eh != null) {
                eh(this, e);
            }
        }

        // consider make public
        internal virtual Region GetTransparentRegion(ToolStrip toolStrip) {
             return null;
        }


        protected internal virtual void Initialize(ToolStrip toolStrip){
        }

        protected internal virtual void InitializePanel(ToolStripPanel toolStripPanel){
        }

        protected internal virtual void InitializeContentPanel(ToolStripContentPanel contentPanel){
        }


        protected internal virtual void InitializeItem (ToolStripItem item){
        }

        protected static void ScaleArrowOffsetsIfNeeded() {
            if (isScalingInitialized) {
                return;
            }

            if (DpiHelper.IsScalingRequired) {
                Offset2X = DpiHelper.LogicalToDeviceUnitsX(OFFSET_2PIXELS);
                Offset2Y = DpiHelper.LogicalToDeviceUnitsY(OFFSET_2PIXELS);
                offset4X = DpiHelper.LogicalToDeviceUnitsX(OFFSET_4PIXELS);
                offset4Y = DpiHelper.LogicalToDeviceUnitsY(OFFSET_4PIXELS);
            }
            isScalingInitialized = true;

        }

        protected virtual void OnRenderArrow(ToolStripArrowRenderEventArgs e){

            if (RendererOverride != null) {
               RendererOverride.OnRenderArrow(e);
               return;
            }
            Graphics g = e.Graphics;
            Rectangle dropDownRect = e.ArrowRectangle;
            using (Brush brush = new SolidBrush(e.ArrowColor)) {

                 Point middle = new Point(dropDownRect.Left + dropDownRect.Width / 2, dropDownRect.Top + dropDownRect.Height / 2);
                 // if the width is odd - favor pushing it over one pixel right.
                 //middle.X += (dropDownRect.Width % 2);

                 Point[] arrow = null;

                 ScaleArrowOffsetsIfNeeded();

                // using (offset4X - Offset2X) instead of (Offset2X) to compensate for rounding error in scaling
                int horizontalOffset = DpiHelper.IsScalingRequirementMet ? offset4X - Offset2X : Offset2X;

                switch (e.Direction) {
                     case ArrowDirection.Up:

                         arrow = new Point[] {
                                 new Point(middle.X - Offset2X, middle.Y + 1),
                                 new Point(middle.X + Offset2X + 1, middle.Y + 1),
                                 new Point(middle.X, middle.Y - Offset2Y)};

                         break;
                     case ArrowDirection.Left:
                         arrow = new Point[] {
                                 new Point(middle.X + Offset2X, middle.Y - offset4Y),
                                 new Point(middle.X + Offset2X, middle.Y + offset4Y),
                                 new Point(middle.X - horizontalOffset, middle.Y)};

                         break;
                     case ArrowDirection.Right:
                         arrow = new Point[] {
                                 new Point(middle.X - Offset2X, middle.Y - offset4Y),
                                 new Point(middle.X - Offset2X, middle.Y + offset4Y),
                                 new Point(middle.X + horizontalOffset, middle.Y)};

                         break;
                     case ArrowDirection.Down:
                     default:
                         arrow = new Point[] {
                             new Point(middle.X - Offset2X, middle.Y - 1),
                             new Point(middle.X + Offset2X + 1, middle.Y - 1),
                             new Point(middle.X, middle.Y + Offset2Y) };
                         break;
                 }
                 g.FillPolygon(brush, arrow);
            }
        }

        /// <devdoc>
        /// Draw the ToolStrip background.  ToolStrip users should override this if they want to draw differently.
        /// </devdoc>
        protected virtual void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
            if (RendererOverride != null) {
               RendererOverride.OnRenderToolStripBackground(e);
               return;
            }
        }

        /// <devdoc>
        /// Draw the border around the ToolStrip.  This should be done as the last step.
        /// </devdoc>
        protected virtual void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderToolStripBorder(e);
                return;
            }
        }

        /// <devdoc>
        /// Draw the grip.  ToolStrip users should override this if they want to draw differently.
        /// </devdoc>
        protected virtual void OnRenderGrip(ToolStripGripRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderGrip(e);
                return;
            }
        }

        /// <devdoc>
        /// Draw the items background
        /// </devdoc>
        protected virtual void OnRenderItemBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderItemBackground(e);
                return;
            }

        }

        /// <devdoc>
        /// Draw the items background
        /// </devdoc>
        protected virtual void OnRenderImageMargin(ToolStripRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderImageMargin(e);
                return;
            }

        }
        /// <devdoc>
        /// Draw the button background
        /// </devdoc>
        protected virtual void OnRenderButtonBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderButtonBackground(e);
                return;
            }

        }

        /// <devdoc>
        /// Draw the button background
        /// </devdoc>
        protected virtual void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderDropDownButtonBackground(e);
                return;
            }

        }

        /// <devdoc>
        /// Draw the button background
        /// </devdoc>
        protected virtual void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderOverflowButtonBackground(e);
                return;
            }
        }
        /// <devdoc>
        /// Draw the item'si mage.  ToolStrip users should override this function to change the
        /// drawing of all images.
        /// </devdoc>
        protected virtual void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)  {
            if (RendererOverride != null) {
                RendererOverride.OnRenderItemImage(e);
                return;
            }

            Rectangle imageRect = e.ImageRectangle;
            Image image = e.Image;

            if (imageRect != Rectangle.Empty && image != null) {
                bool disposeImage = false;
                if (e.ShiftOnPress && e.Item.Pressed) {
                   imageRect.X++;
                }
                if (!e.Item.Enabled) {
                    image = CreateDisabledImage(image, e.ImageAttributes);
                    disposeImage = true;
                }
                if (e.Item.ImageScaling == ToolStripItemImageScaling.None) {
                   e.Graphics.DrawImage(image, imageRect, new Rectangle(Point.Empty,imageRect.Size), GraphicsUnit.Pixel);

                }
                else {
                    e.Graphics.DrawImage(image, imageRect);
                }

                if (disposeImage) {
                    image.Dispose();
                }
            }
        }


        protected virtual void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)  {
            if (RendererOverride != null) {
                RendererOverride.OnRenderItemCheck(e);
                return;
            }

           Rectangle imageRect = e.ImageRectangle;
           Image image = e.Image;

           if (imageRect != Rectangle.Empty && image != null) {
               if (!e.Item.Enabled) {
                   image = CreateDisabledImage(image, e.ImageAttributes);
               }

               e.Graphics.DrawImage(image, imageRect, 0, 0, imageRect.Width,
                   imageRect.Height, GraphicsUnit.Pixel, e.ImageAttributes);
           }
       }


        /// <devdoc>
        /// Draw the item's text.  ToolStrip users should override this function to change the
        /// drawing of all text.
        /// </devdoc>
        protected virtual void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderItemText(e);
                return;
            }

            ToolStripItem item         = e.Item;
            Graphics g                 = e.Graphics;
            Color textColor            = e.TextColor;
            Font textFont              = e.TextFont;
            string text                = e.Text;
            Rectangle textRect         = e.TextRectangle;
            TextFormatFlags textFormat = e.TextFormat;
            // if we're disabled draw in a different color.
            textColor = (item.Enabled) ? textColor : SystemColors.GrayText;

            if (e.TextDirection != ToolStripTextDirection.Horizontal && textRect.Width > 0 && textRect.Height > 0) {
                // Perf: this is a bit heavy handed.. perhaps we can share the bitmap.
                Size textSize = LayoutUtils.FlipSize(textRect.Size);
                using (Bitmap textBmp = new Bitmap(textSize.Width, textSize.Height,PixelFormat.Format32bppPArgb)) {

                    using (Graphics textGraphics = Graphics.FromImage(textBmp)) {
                        // now draw the text..
                        textGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                        TextRenderer.DrawText(textGraphics, text, textFont, new Rectangle(Point.Empty, textSize), textColor, textFormat);
                        textBmp.RotateFlip((e.TextDirection == ToolStripTextDirection.Vertical90) ? RotateFlipType.Rotate90FlipNone :  RotateFlipType.Rotate270FlipNone);
                        g.DrawImage(textBmp, textRect);
                    }
                }
            }
            else {
                TextRenderer.DrawText(g, text, textFont, textRect, textColor, textFormat);
            }
        }

        /// <devdoc>
        /// Draw the button background
        /// </devdoc>
        protected virtual void OnRenderLabelBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderLabelBackground(e);
                return;
            }


        }
        /// <devdoc>
        /// Draw the items background
        /// </devdoc>
        protected virtual void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
             if (RendererOverride != null) {
                 RendererOverride.OnRenderMenuItemBackground(e);
                 return;
             }
        }

        /// <devdoc>
        /// Draws a toolbar separator. ToolStrip users should override this function to change the
        /// drawing of all separators.
        /// </devdoc>
        protected virtual void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderSeparator(e);
                return;
            }

        }

        protected virtual void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderToolStripPanelBackground(e);
                return;
            }
        }

        protected virtual void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderToolStripContentPanelBackground(e);
                return;
            }
        }

        protected virtual void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderToolStripStatusLabelBackground(e);
                return;
            }
        }

        protected virtual void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderStatusStripSizingGrip(e);
                return;
            }

            Graphics g = e.Graphics;
            StatusStrip statusStrip = e.ToolStrip as StatusStrip;

            // we have a set of stock rectangles.  Translate them over to where the grip is to be drawn
            // for the white set, then translate them up and right one pixel for the grey.


            if (statusStrip != null) {
                Rectangle sizeGripBounds = statusStrip.SizeGripBounds;

                if (!LayoutUtils.IsZeroWidthOrHeight(sizeGripBounds)) {
                    Rectangle[] whiteRectangles = new Rectangle[baseSizeGripRectangles.Length];
                    Rectangle[] greyRectangles = new Rectangle[baseSizeGripRectangles.Length];

                    for (int i = 0; i < baseSizeGripRectangles.Length; i++) {
                        Rectangle baseRect = baseSizeGripRectangles[i];
                        if (statusStrip.RightToLeft == RightToLeft.Yes) {
                            baseRect.X = sizeGripBounds.Width - baseRect.X - baseRect.Width;
                        }
                        baseRect.Offset(sizeGripBounds.X, sizeGripBounds.Bottom - 12 /*height of pyramid (10px) + 2px padding from bottom*/);
                        whiteRectangles[i] = baseRect;
                        if (statusStrip.RightToLeft == RightToLeft.Yes) {
                            baseRect.Offset(1, -1);
                        }
                        else {
                            baseRect.Offset(-1, -1);
                        }
                        greyRectangles[i] = baseRect;

                    }

                    g.FillRectangles(SystemBrushes.ButtonHighlight, whiteRectangles);
                    g.FillRectangles(SystemBrushes.ButtonShadow, greyRectangles);
                }
            }

        }
        /// <devdoc>
        /// Draw the item's background.
        /// </devdoc>
        protected virtual void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e) {
            if (RendererOverride != null) {
                RendererOverride.OnRenderSplitButtonBackground(e);
                return;
            }
        }

        // Only paint background effects if no backcolor has been set or no background image has been set.
        internal bool ShouldPaintBackground (Control control) {
            return (control.RawBackColor == Color.Empty && control.BackgroundImage == null);
        }

        private static Image CreateDisabledImage(Image normalImage, ImageAttributes imgAttrib) {
            if (imgAttrib == null) {
                imgAttrib = new ImageAttributes();
            }

            imgAttrib.ClearColorKey();
            imgAttrib.SetColorMatrix(DisabledImageColorMatrix);

            Size size = normalImage.Size;
            Bitmap disabledBitmap = new Bitmap(size.Width, size.Height);
            using (Graphics graphics = Graphics.FromImage(disabledBitmap)) {

                graphics.DrawImage(normalImage,
                                   new Rectangle(0, 0, size.Width, size.Height),
                                   0, 0, size.Width, size.Height,
                                   GraphicsUnit.Pixel,
                                   imgAttrib);
            }

            return disabledBitmap;
        }

        }
}
