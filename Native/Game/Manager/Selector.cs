namespace FFF7RCore {
    using System.Collections.Generic;

    public class Selector
    {
        public int Count => cardCodeList_.Count;
        private List<string> cardCodeList_;

        public Selector(List<string> cardCodeList = null) {
            if (cardCodeList == null)
            {
                cardCodeList_ = new List<string>();
            }
            else
            {
                cardCodeList_ = cardCodeList;
            }
        }

        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= cardCodeList_.Count)
                {
                    return null;
                }
                return cardCodeList_[index];
            }
            set
            {
                cardCodeList_[index] = value;
            }
        }

        public List<string> ChessPool {
            get
            {
                return cardCodeList_;
            }
            set
            {
                cardCodeList_ = value;
            }
        }

        public bool Add(string cardCode)
        {
            if (string.IsNullOrWhiteSpace(cardCode))
            {
                return false;
            }
            cardCodeList_.Add(cardCode);
            return true;
        }

        public bool RemoveAt(int index) {
            if (index < 0 || index >= cardCodeList_.Count) {
                return false;
            }
            cardCodeList_.RemoveAt(index);
            return true;
        }

        public bool RemoveAt(List<int> indexes) {
            bool result = true;
            for(int i = 0; i < indexes.Count; i++) {
                if (!RemoveAt(indexes[i])) {
                    result = false;
                }
            }
            return result;
        }

        public bool Remove(string chessOne) {
            bool result = cardCodeList_.Remove(chessOne);
            return result;
        }
    }
}