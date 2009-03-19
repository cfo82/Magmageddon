/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace xWinFormsLib
{
    public class Listview : Control
    {
        string[,] item;
        List<Header> columnHeader = new List<Header>();
        public List<Header> ColumnHeader { get { return columnHeader; } set { columnHeader = value; } }
        
        Rectangle headerArea = Rectangle.Empty;
        bool isResizing = false;
        public class Header
        {
            string text;
            float width;
            public EventHandler OnPress = null;
            public EventHandler OnRelease = null;
            public string Text { get { return text; } set { text = value; } }
            public float Width { get { return width; } set { width = value; } }
            public Header(string text, float width)
            {
                this.text = text;
                this.width = width;
            }
        }

        Texture2D pixel;
        Rectangle backArea;

        Scrollbar hScrollbar, vScrollbar;
        private int VisibleItems
        {
            get
            {
                int visibleItems = 0;
                visibleItems = (int)System.Math.Floor(Height / Font.LineSpacing);

                if (hScrollbar.Max > 0)
                    visibleItems -= 1;
                if (headerStyle != ListviewHeaderStyle.None)
                    visibleItems -= 1;

                return visibleItems;
            }
        }

        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;
        Texture2D capturedTexture;
        Rectangle srcRect = Rectangle.Empty;

        ListviewHeaderStyle headerStyle = ListviewHeaderStyle.Clickable;
        public ListviewHeaderStyle HeaderStyle { get { return headerStyle; } set { headerStyle = value; } }
        public enum ListviewHeaderStyle
        {
            Clickable,
            Nonclickable,
            None
        }

        int headerHoverIndex = -1;

        bool gridLines = false;
        public bool GridLines { get { return gridLines; } set { gridLines = value; } }
        Rectangle lineRect = Rectangle.Empty;
        Point gridSize = Point.Zero;

        int selectedRow = -1;
        int selectedColumn = -1;
        int hoverRowIndex = -1;
        int hoverColumnIndex = -1;
        Rectangle selectionArea = Rectangle.Empty;
        Rectangle selectionRect = Rectangle.Empty;
        bool fullRowSelect = false;
        public bool FullRowSelect { get { return fullRowSelect; } set { fullRowSelect = value; } }
        bool hoverSelection = false;
        public bool HoverSelection { get { return hoverSelection; } set { hoverSelection = value; } }
        public EventHandler OnSelect = null;

        public Listview(string name, Vector2 position, Vector2 size, string[,] item)
            : base(name, position)
        {
            this.Size = size;
            this.item = item;
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            Height = (int)((Size.Y - 2 + 12) / (float)Font.LineSpacing) * Font.LineSpacing;

            area.Width = (int)Width;
            area.Height = (int)Height;            

            backArea = area;
            pixel = new Texture2D(graphics, 1, 1);
            pixel.SetData<Color>(new Color[] { Color.White });

            #region Init Scrollbars

            vScrollbar = new Scrollbar("vScrollbar", new Vector2(Position.X + Width - 13, Position.Y + 1), Scrollbar.Type.Vertical, Height - 14);
            vScrollbar.Owner = Owner;
            vScrollbar.Initialize(content, graphics);

            hScrollbar = new Scrollbar("hScrollbar", new Vector2(Position.X + 1, Position.Y + Height - 13), Scrollbar.Type.Horizontal, Width - 14);
            hScrollbar.Owner = Owner;
            hScrollbar.Initialize(content, graphics);

            #endregion

            spriteBatch = new SpriteBatch(graphics);
            renderTarget = new RenderTarget2D(graphics, area.Width - 2, area.Height - 2, 1, SurfaceFormat.Color);
            capturedTexture = new Texture2D(graphics, area.Width - 2, area.Height - 2);

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            hScrollbar.Dispose();
            vScrollbar.Dispose();

            pixel.Dispose();
            renderTarget.Dispose();
            capturedTexture.Dispose();
            spriteBatch.Dispose();

            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            hScrollbar.Update(gameTime);
            vScrollbar.Update(gameTime);

            if (isResizing && MouseHelper.IsReleased)
                    isResizing = false;

            selectionArea.X = (int)(Owner.Position.X + Position.X);
            selectionArea.Y = (int)(Owner.Position.Y + Position.Y);
            selectionArea.Width = (int)area.Width;
            selectionArea.Height = (int)area.Height;

            if (headerStyle != ListviewHeaderStyle.None)
            {
                selectionArea.Y += Font.LineSpacing;
                selectionArea.Height -= Font.LineSpacing;
            }

            if (vScrollbar.Max > 0)
                selectionArea.Width -= 12;
            if (hScrollbar.Max > 0)
                selectionArea.Height -= 12;

            if (hoverRowIndex != -1)
                UpdateSelection();

            //ColumnHeader_OnRelease
            if (headerStyle == ListviewHeaderStyle.Clickable && headerHoverIndex != -1 && 
                MouseHelper.HasBeenReleased && columnHeader[headerHoverIndex].OnRelease != null)
            {
                columnHeader[headerHoverIndex].OnRelease(null, null);
                headerHoverIndex = -1;
            }
        }

        private void UpdateSelection()
        {
            if (MouseHelper.HasBeenPressed)
            {
                selectedRow = hoverRowIndex;
                selectedColumn = hoverColumnIndex;

                if (OnSelect != null)
                    if (fullRowSelect)
                        OnSelect(selectedRow, null);
                    else
                        OnSelect(item[selectedRow, selectedColumn], null);
            }

            hoverRowIndex = -1;
            hoverColumnIndex = -1;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBorder(spriteBatch);

            Render(FormCollection.Graphics.GraphicsDevice);

            if (hScrollbar.Max > 0)
                srcRect.Height = area.Height - 14;
            else
                srcRect.Height = area.Height - 2;

            if (vScrollbar.Max > 0)
                srcRect.Width = area.Width - 14;
            else
                srcRect.Width = area.Width - 2;

            spriteBatch.Draw(capturedTexture, Position + Vector2.One, srcRect, Color.White);
            //spriteBatch.Draw(pixel, selectionArea, Color.LightSkyBlue);

            DrawScrollbars(spriteBatch);
        }

        private void DrawBorder(SpriteBatch spriteBatch)
        {
            backArea.X = (int)Position.X;
            backArea.Y = (int)Position.Y;
            backArea.Width = (int)Width;
            backArea.Height = (int)Height;
            spriteBatch.Draw(pixel, backArea, Color.Black);

            backArea.X += 1;
            backArea.Y += 1;
            backArea.Width -= 2;
            backArea.Height -= 2;
            spriteBatch.Draw(pixel, backArea, Color.White);
        }

        private void DrawScrollbars(SpriteBatch spriteBatch)
        {
            int totalWidth = 0;
            if (columnHeader.Count > 0)
                for (int i = 0; i < columnHeader.Count; i++)
                    totalWidth += (int)columnHeader[i].Width;

            if (!isResizing)
            {
                if (vScrollbar.Max == 0)
                    hScrollbar.Max = totalWidth - area.Width;
                else
                    hScrollbar.Max = totalWidth - (area.Width - 12);
            }
            hScrollbar.Step = System.Math.Max(1, hScrollbar.Max / 15);

            if (hScrollbar.Max > 0)
            {
                if (vScrollbar.Max > 0 && hScrollbar.Width != Width - 14)
                    hScrollbar.Width = Width - 14;
                else if (vScrollbar.Max == 0 && hScrollbar.Width != Width - 2)
                    hScrollbar.Width = Width - 2;

                hScrollbar.Draw(spriteBatch);
            }

            int rowCount = item.GetLength(0);
            if (rowCount > VisibleItems)
                vScrollbar.Max = rowCount - VisibleItems;
            else
                vScrollbar.Max = 0;

            if (vScrollbar.Max > 0)
            {
                if (hScrollbar.Max > 0 && vScrollbar.Height != Height - 14)
                    vScrollbar.Height = Height - 14;
                else if (hScrollbar.Max == 0 && vScrollbar.Height != Height - 2)
                    vScrollbar.Height = Height - 2;

                vScrollbar.Draw(spriteBatch);
            }
        }

        private void Render(GraphicsDevice graphics)
        {
            graphics.SetRenderTarget(0, renderTarget);
            graphics.Clear(Color.TransparentBlack);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            DrawColumnHeaders(spriteBatch);
            DrawItems(spriteBatch);
            DrawGridLines(spriteBatch);

            spriteBatch.End();

            graphics.SetRenderTarget(0, null);
            capturedTexture = renderTarget.GetTexture();
            graphics.Clear(Color.TransparentBlack);
        }

        private void DrawColumnHeaders(SpriteBatch spriteBatch)
        {
            if (headerStyle != ListviewHeaderStyle.None)
            {
                headerArea.X = -hScrollbar.Value;
                headerArea.Y = 0;

                Vector2 textPos = Vector2.Zero;
                int totalWidth = 0;

                for (int i = 0; i < columnHeader.Count; i++)
                {
                    headerArea.Width = (int)columnHeader[i].Width;
                    headerArea.Height = Font.LineSpacing;
                    totalWidth += headerArea.Width;

                    UpdateHeaderSize(headerArea, i, spriteBatch);
                    
                    spriteBatch.Draw(pixel, headerArea, Color.Black);

                    #region MouseOver Bevel Effect
                    
                    Rectangle hoverArea = headerArea;
                    hoverArea.X = (int)(headerArea.X + Position.X + Owner.Position.X);
                    hoverArea.Y = (int)(headerArea.Y + Position.Y + Owner.Position.Y);

                    if (headerStyle == ListviewHeaderStyle.Clickable && hoverArea.Contains(MouseHelper.Cursor.Location) && Owner == FormCollection.ActiveForm)
                    {
                        headerHoverIndex = i;

                        //ColumnHeader_OnPress
                        if (MouseHelper.HasBeenPressed && columnHeader[i].OnPress != null)
                            columnHeader[i].OnPress(null, null);

                        if (MouseHelper.IsPressed)
                        {
                            headerArea.Width -= 1;
                            headerArea.Height -= 1;
                            spriteBatch.Draw(pixel, headerArea, Color.DarkGray);

                            headerArea.X += 1;
                            headerArea.Y += 1;
                            headerArea.Width -= 1;
                            headerArea.Height -= 1;
                            spriteBatch.Draw(pixel, headerArea, Color.LightGray);

                            headerArea.X += 1;
                            headerArea.Y -= 1;
                        }
                        else
                        {
                            #region Draw normal header
                            headerArea.Width -= 1;
                            headerArea.Height -= 1;
                            spriteBatch.Draw(pixel, headerArea, Color.White);

                            headerArea.X += 1;
                            headerArea.Y += 1;
                            headerArea.Width -= 1;
                            headerArea.Height -= 1;
                            spriteBatch.Draw(pixel, headerArea, Color.LightGray);

                            headerArea.X += 1;
                            headerArea.Y -= 1;
                            #endregion
                        }
                    }
                    else
                    {
                        #region Draw normal header
                        headerArea.Width -= 1;
                        headerArea.Height -= 1;
                        spriteBatch.Draw(pixel, headerArea, Color.White);

                        headerArea.X += 1;
                        headerArea.Y += 1;
                        headerArea.Width -= 1;
                        headerArea.Height -= 1;
                        spriteBatch.Draw(pixel, headerArea, Color.LightGray);

                        headerArea.X += 1;
                        headerArea.Y -= 1;
                        #endregion
                    }

                    #endregion

                    if (Font.MeasureString(columnHeader[i].Text).X > headerArea.Width - 20)
                    {
                        #region truncate text
                        string text = columnHeader[i].Text;
                        for (int j = 0; j < text.Length; j++)
                            if (Font.MeasureString(text.Substring(0, j)).X > headerArea.Width - 20)
                            {
                                if (j > 0)
                                    text = text.Substring(0, j - 1) + "..";
                                else
                                    text = "";
                                break;
                            }

                        textPos.X = (int)(headerArea.X + (headerArea.Width - Font.MeasureString(text).X) / 2f);
                        textPos.Y = 0;
                        spriteBatch.DrawString(Font, text, textPos, Color.Black);
                        #endregion
                    }
                    else
                    {
                        textPos.X = (int)(headerArea.X + (headerArea.Width - Font.MeasureString(columnHeader[i].Text).X) / 2f);
                        textPos.Y = 0;
                        spriteBatch.DrawString(Font, columnHeader[i].Text, textPos, Color.Black);
                    }

                    UpdateHeaderSize(headerArea, i, spriteBatch);

                    headerArea.X += headerArea.Width;                    
                }

                if(hScrollbar.Max == 0)
                    DrawHeaderEnd(spriteBatch, area.Width - totalWidth);
            }
        }

        Rectangle sizeArea = Rectangle.Empty;
        int resizeIndex = -1;        
        private void UpdateHeaderSize(Rectangle headerArea, int index, SpriteBatch spriteBatch)
        {
            if (Owner == FormCollection.TopMostForm)
            {
                sizeArea.X = headerArea.X + headerArea.Width - 5 + (int)Position.X;
                sizeArea.Y = headerArea.Y + (int)Position.Y;
                sizeArea.Width = 10;
                sizeArea.Height = headerArea.Height;

                if (Owner != null)
                {
                    sizeArea.X += (int)Owner.Position.X;
                    sizeArea.Y += (int)Owner.Position.Y;
                }

                if (sizeArea.Contains(MouseHelper.Cursor.Location))
                {
                    if (MouseHelper.HasBeenPressed)
                    {
                        isResizing = true;
                        resizeIndex = index;
                    }
                }
            }

            if (isResizing && resizeIndex != -1)
            {
                if (MouseHelper.IsPressed)
                {
                    float newWidth = MouseHelper.Cursor.Location.X - (Position.X + Owner.Position.X - hScrollbar.Value);

                    if(resizeIndex > 0)
                        for (int i = resizeIndex - 1; i > -1; i--)
                        {
                            newWidth -= columnHeader[i].Width;
                        }

                    if (newWidth < 2)
                        newWidth = 2;

                    columnHeader[resizeIndex].Width = newWidth;
                }
                else if (MouseHelper.HasBeenReleased)
                {
                    isResizing = false;
                    resizeIndex = -1;
                }
            }
        }

        private void DrawHeaderEnd(SpriteBatch spriteBatch, int width)
        {
            headerArea.Width = (int)width;
            headerArea.Height = Font.LineSpacing;
            spriteBatch.Draw(pixel, headerArea, Color.Black);

            headerArea.Width -= 1;
            headerArea.Height -= 1;
            spriteBatch.Draw(pixel, headerArea, Color.White);

            headerArea.X += 1;
            headerArea.Y += 1;
            headerArea.Width -= 1;
            headerArea.Height -= 1;
            spriteBatch.Draw(pixel, headerArea, Color.LightGray);
        }

        private void DrawGridLines(SpriteBatch spriteBatch)
        {
            lineRect.X = 0;
            lineRect.Y = 0;

            gridSize.X = columnHeader.Count + 1;
            gridSize.Y = area.Height / Font.LineSpacing;
            if(headerStyle != ListviewHeaderStyle.None)
                gridSize.Y -=1;

            lineRect.Width = area.Width;
            lineRect.Height = 1;
            for (int y = 1; y < gridSize.Y; y++)
            {
                if (headerStyle != ListviewHeaderStyle.None)
                    lineRect.Y = y * Font.LineSpacing + Font.LineSpacing;
                else
                    lineRect.Y = y * Font.LineSpacing;

                spriteBatch.Draw(pixel, lineRect, Color.LightGray);
            }

            lineRect.Width = 1;
            lineRect.Height = area.Height;
            if (headerStyle != ListviewHeaderStyle.None)
                lineRect.Y = Font.LineSpacing;
            else
                lineRect.Y = 0;

            lineRect.X = -1 - hScrollbar.Value;
            for (int x = 0; x < columnHeader.Count; x++)
            {
                spriteBatch.Draw(pixel, lineRect, Color.LightGray);
                lineRect.X += (int)columnHeader[x].Width;
            }

            spriteBatch.Draw(pixel, lineRect, Color.LightGray);
        }

        private void DrawItems(SpriteBatch spriteBatch)
        {
            Vector2 textPos = Vector2.Zero;
            for (int y = 0; y < item.GetLength(0); y++)
            {
                textPos.X = -hScrollbar.Value + 4;
                for (int x = 0; x < item.GetLength(1); x++)
                {
                    if (y > vScrollbar.Value - 1)
                    {
                        if (headerStyle == ListviewHeaderStyle.None)
                            textPos.Y = (y - vScrollbar.Value) * Font.LineSpacing;
                        else
                            textPos.Y = (y - vScrollbar.Value) * Font.LineSpacing + Font.LineSpacing;

                        if (fullRowSelect && x == 0 && y == selectedRow)
                            DrawSelectedBox(new Vector2(0f, textPos.Y));
                        else if(!fullRowSelect && x == selectedRow && y == selectedColumn)
                            DrawSelectedBox(textPos);

                        if (!isResizing)
                            DrawSelectionBox(textPos, x, y);

                        if (item[y, x] != null && (columnHeader.Count == 0 || (x < columnHeader.Count && Font.MeasureString(item[y, x]).X > columnHeader[x].Width - 15)))
                        {
                            #region truncate text
                            string text = item[y, x];
                            for (int i = 0; i < text.Length; i++)
                                if (columnHeader.Count == 0 || (x < columnHeader.Count && Font.MeasureString(text.Substring(0, i)).X > columnHeader[x].Width - 10))
                                {
                                    if (i > 0)
                                        text = text.Substring(0, i - 1) + "..";
                                    else
                                        text = "";
                                }

                            if (x < columnHeader.Count)
                                spriteBatch.DrawString(Font, text, textPos, Color.Black);
                            #endregion
                        }
                        else if (item[y, x] != null && x < columnHeader.Count)
                            spriteBatch.DrawString(Font, item[y, x], textPos, Color.Black);
                    }

                    if (columnHeader.Count > 0 && x < columnHeader.Count)
                        textPos.X += columnHeader[x].Width;
                }
            }
        }

        private void DrawSelectionBox(Vector2 position, int x, int y)
        {
            if (fullRowSelect)
                selectionRect.Width = srcRect.Width + 2;
            else
                selectionRect.Width = (int)columnHeader[x].Width + 2;
            selectionRect.Height = Font.LineSpacing;

            selectionRect.X = (int)(position.X + Owner.Position.X + Position.X - 4);
            selectionRect.Y = (int)(position.Y + Owner.Position.Y + Position.Y);

            if (selectionArea.Contains(MouseHelper.Cursor.Location) && 
                selectionRect.Contains(MouseHelper.Cursor.Location) && 
                FormCollection.TopMostForm == Owner)
            {
                if (fullRowSelect)
                {
                    hoverRowIndex = y;
                    hoverColumnIndex = x;
                }
                else
                {
                    hoverRowIndex = x;
                    hoverColumnIndex = y;
                }

                if (hoverSelection)
                {
                    selectionRect.X = (int)position.X - 4;
                    selectionRect.Y = (int)position.Y;
                    spriteBatch.Draw(pixel, selectionRect, Color.LightGray);
                }                
            }            
        }

        private void DrawSelectedBox(Vector2 position)
        {
            if (fullRowSelect)
            {
                selectionRect.X = 0;
                selectionRect.Width = area.Width;
            }
            else
            {
                selectionRect.X = (int)position.X - 4;
                selectionRect.Width = (int)columnHeader[selectedRow].Width;
            }

            selectionRect.Height = Font.LineSpacing;
            selectionRect.Y = (int)position.Y;

            spriteBatch.Draw(pixel, selectionRect, Color.Silver);
        }
    }
}
