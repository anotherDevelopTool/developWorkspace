// Hamed JI - 2006/08/29 - 1385/06/07
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace FilterControls
{
    public class FilterTextBox : TextBox
    {
        private string _ForbidenChars;
        private bool _InternalEditing;
        private bool _CaptleLetterOnly;
        private AcceptableCharacters _AcceptableChar;

        public enum AcceptableCharacters
        {
            LetterOnly = 0,
            DigitOnly,
            LetterOrDigit,
            All
        }

        public FilterTextBox()
        {
            _ForbidenChars = string.Empty;
            _InternalEditing = false;
            _CaptleLetterOnly = false;
            _AcceptableChar = AcceptableCharacters.All;
        }

        #region " Appearance "

        /// <summary>
        /// Indicate wich characters can't be in string
        /// </summary>
        [Category("Behavior"),DefaultValue(""),
            Description("Indicate wich characters can't be in string")]
        public string ForbidenChars
        {
            get
            {
                return _ForbidenChars;
            }
            set
            {
                _ForbidenChars = value;
                Text = Text;
            }
        }

        /// <summary>
        /// Indicate only Captle characters can write
        /// </summary>
        [Category("Behavior"), DefaultValue(false),
            Description("Indicate only Captle characters can write"), Browsable(true)]
        public bool CaptleLettersOnly
        {
            get
            {
                return _CaptleLetterOnly;
            }
            set
            {
                _CaptleLetterOnly = value;
                Text = Text;
            }
        }

        /// <summary>
        /// Indicate wich type of characters are acceptable
        /// </summary>
        [Category("Behavior"), DefaultValue(FilterTextBox.AcceptableCharacters.All),
            Description("Indicate wich type of characters are acceptable"), Browsable(true)]
        public AcceptableCharacters AcceptableChars
        {
            get
            {
                return _AcceptableChar;
            }
            set
            {
                _AcceptableChar = value;
                Text = Text;
            }
        }
        #endregion

        #region " Overrides "

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (_InternalEditing == true)
                    base.Text = value;
                else
                    base.Text = RemoveForbidens(value);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            int SelS = this.SelectionStart;
            _InternalEditing = true;
            Text = RemoveForbidens(Text, ref SelS);
            _InternalEditing = false;
            this.SelectionStart = SelS;
        }
        #endregion

        private string RemoveForbidens(string st, ref int SelStart)
        {
            if (_CaptleLetterOnly == true)
                st = st.ToUpper();

            for (int i = st.Length - 1; i >= 0; i--)
            {
                if (_ForbidenChars.IndexOf(st[i]) != -1)
                {
                    st = st.Remove(i, 1);
                    if (i < SelStart)
                        SelStart--;
                }
                else if (_AcceptableChar == AcceptableCharacters.DigitOnly && char.IsDigit(st[i]) != true)
                {
                    st = st.Remove(i, 1);
                    if (i < SelStart)
                        SelStart--;
                }
                else if (_AcceptableChar == AcceptableCharacters.LetterOnly && char.IsLetter(st[i]) != true)
                {
                    st = st.Remove(i, 1);
                    if (i < SelStart)
                        SelStart--;
                }
                else if (_AcceptableChar == AcceptableCharacters.LetterOrDigit && char.IsLetterOrDigit(st[i]) != true)
                {
                    st = st.Remove(i, 1);
                    if (i < SelStart)
                        SelStart--;
                }
            }
            return st;
        }

        private string RemoveForbidens(string st)
        {
            int i = st.Length;
            return RemoveForbidens(st, ref i);
        }
    }
}