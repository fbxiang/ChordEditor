﻿using common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chordgen
{
    class ChromaVisualizer
    {
        private Timeline TL;
        private BeatEditor BeatEditor;
        private SongInfo Info;
        private Bitmap[] chromaBitmaps;
        public int ChromaStart = 75;
        public int ChromaHeight = 10;
        public int ChromaTextHeight = 15;

        public bool Prepared;
        Font tonaltyFont = SystemFonts.CaptionFont;
        Brush tonaltyBrush = Brushes.LightGreen;
        Font noteFont = SystemFonts.CaptionFont;
        Brush noteFrontBrush = Brushes.White;
        Brush noteBackBrush = Brushes.Black;
        Pen chromaSelectPen = Pens.LightGray;
        int chromaTextStart = 71;
        int fontShadowDX = 1, fontShadowDY = 1;

        public enum TimelineChromaMode
        {
            FrameFull = 0,
            FrameScale=1,
            //Bar = 1,
            Global = 2,
            None = 3
        }
        public TimelineChromaMode ChromaMode;

        public ChromaVisualizer(Timeline tl)
        {
            TL = tl;
            Info = TL.Info;
            BeatEditor = TL.BeatEditor;
        }

        public Tonalty GetCurrentTonalty()
        {
            int id = BeatEditor.GetPreviousBeatID(Program.TL.CurrentTime);
            if (id == -1) id = 0;
            return Info.beats[id].Tonalty;
        }

        public void ChangeChromaMode()
        {
            ChromaMode++;
            if (ChromaMode > TimelineChromaMode.None) ChromaMode = 0;
        }
        public void PrepareChromaFrameImage()
        {
            if (Info.chroma == null) return;
            chromaBitmaps = new Bitmap[12];
            for (int j = 0; j < 12; ++j)
                chromaBitmaps[j] = new Bitmap(Info.chroma.Frames.Length, 1);
            int pos = 0;
            foreach (Chroma.ChromaFrame frame in Info.chroma.Frames)
            {
                for (int j = 0; j < 12; ++j)
                {
                    int percent = (int)((frame.D[j] / Info.chroma.GlobalMax) * 255);
                    chromaBitmaps[j].SetPixel(pos, 0, Color.FromArgb(percent, percent, percent));
                }
                ++pos;
            }
            Prepared = true;
        }
        public void DrawTonalty()
        {
            double tempLeftMostTime = TL.LeftMostTime, tempRightMostTime = TL.RightMostTime;
            int left = BeatEditor.GetPreviousBeatID(tempLeftMostTime) - 1, right = BeatEditor.GetNextBeatID(tempRightMostTime);
            // Get the previous of previous beat of the left bound and the next beat of the right bound.
            if (left < 0) left = 0;
            // Tonalty of the last beat is wrong and useless
            if (right >= Info.beats.Count - 1) right = Info.beats.Count - 2;
            Tonalty lastTonalty = null;
            int rightPos = TL.TargetRightPos;
            for (int i=right;i>=left;--i)
            {
                int pos = TL.Time2Pos(Info.beats[i].Time);
                if (pos <= 0)
                {
                    lastTonalty = Info.beats[i].Tonalty;
                    break;
                }
                if(i==0||Info.beats[i-1].Tonalty.ToString()!=Info.beats[i].Tonalty.ToString())
                {
                    DrawTonaltyAt(Info.beats[i].Tonalty, pos, rightPos - pos);
                    rightPos = pos;
                }
            }
            if (lastTonalty != null)
                DrawTonaltyAt(lastTonalty, 0, rightPos);
        }
        public void DrawTonaltyAt(Tonalty tonalty,int graphicPosition,int restrictedLength)
        {
            TL.G.DrawString(tonalty.ToString(), tonaltyFont, tonaltyBrush,
                new Rectangle(graphicPosition, 0, restrictedLength, ChromaTextHeight));
            if (tonalty == null) return;
            for (int j = 0; j < 12; ++j)
            {
                if (ChromaMode != TimelineChromaMode.FrameScale || tonalty.IsOnNaturalScale(j))
                {
                    TL.G.DrawString(tonalty.NoteNameUnderTonalty(j), noteFont, noteBackBrush,
                        new Rectangle(graphicPosition + fontShadowDX, chromaTextStart + (11 - j) * ChromaHeight + fontShadowDY, restrictedLength, ChromaTextHeight));
                    TL.G.DrawString(tonalty.NoteNameUnderTonalty(j), noteFont, noteFrontBrush,
                        new Rectangle(graphicPosition, chromaTextStart + (11 - j) * ChromaHeight, restrictedLength, ChromaTextHeight));
                }
            }
        }
        public void DrawChroma()
        {
            if (Info.chroma == null) return;
            int lpos = TL.Time2Pos(0);
            if (lpos < 0) lpos = 0;
            Tonalty tonalty = GetCurrentTonalty();
            if (ChromaMode !=TimelineChromaMode.None)
            {
                if(Prepared)
                {
                    int pos1 = TL.Time2Pos(0);
                    int pos2 = TL.Time2Pos(Info.MP3Length);
                    for (int j = 0; j < 12; ++j)
                    {
                        if (ChromaMode!=TimelineChromaMode.FrameScale || tonalty.IsOnNaturalScale(j))
                        {
                            if (ChromaMode == TimelineChromaMode.Global)
                            {
                                int percent = (int)((Info.GlobalChroma[j] / Info.MaxGlobalChroma) * 255);
                                TL.G.FillRectangle(new SolidBrush(Color.FromArgb(percent, percent, percent)), new Rectangle(0, ChromaStart + (11 - j) * ChromaHeight, TL.TargetRightPos, ChromaHeight));
                            }
                            else
                            {
                                TL.G.DrawImage(chromaBitmaps[j], new Rectangle(pos1, ChromaStart + (11 - j) * ChromaHeight, pos2 - pos1, ChromaHeight));
                            }
                        }
                    }
                }

                /*foreach (Chroma.ChromaFrame frame in Info.chroma.Frames)
                {
                    double time = frame.Time;
                    int pos = TL.Time2Pos(time);
                    int len = (int)(Info.chroma.FrameLength * TL.TimeScale + 1);
                    if (pos + len >= 0 && pos <= TL.TargetRightPos)
                    {
                        for (int j = 0; j < 12; ++j)
                        {
                            int percent = (int)((frame.D[j] / Info.chroma.GlobalMax) * 255);
                            TL.G.FillRectangle(new SolidBrush(Color.FromArgb(percent, percent, percent)), new Rectangle(pos, ChromaStart + (11 - j) * ChromaHeight, len, ChromaHeight));
                        }
                    }
                }*/
            }
            if(TL.IsMouseInControl&&TL.CurrentMouseMode==Timeline.MouseMode.Chroma&& ChromaMode != TimelineChromaMode.None)
            {
                int id = GetChromaID(TL.MousePosY);
                if (ChromaMode != TimelineChromaMode.FrameScale || tonalty.IsOnNaturalScale(id))
                {
                    int pos1 = Math.Max(0, TL.Time2Pos(0));
                    int pos2 = Math.Max(TL.TargetRightPos - 1, TL.Time2Pos(Info.MP3Length));
                    TL.G.DrawRectangle(chromaSelectPen, new Rectangle(pos1, ChromaStart + (11 - id) * ChromaHeight, pos2 - pos1, ChromaHeight));
                }
            }
        }
        private int GetChromaID(int y)
        {
            return 11 - (y - ChromaStart) / ChromaHeight;
        }
        public void ClickOnChromas(int x, int y)
        {
            Tonalty tonalty = GetCurrentTonalty();
            if (ChromaMode != TimelineChromaMode.None)
            {
                int id = GetChromaID(y);
                if (ChromaMode != TimelineChromaMode.FrameScale || tonalty.IsOnNaturalScale(id))
                    Program.MidiManager.PlaySingleNote(id);
            }
        }
    }
}
