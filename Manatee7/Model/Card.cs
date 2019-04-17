using System;
using System.Collections.Generic;
using Manatee7.Model;
using System.Linq;
using Newtonsoft.Json;

namespace Manatee7.Model {

    [Serializable]
    public class Card : IEquatable<Card> {

        public string ID { get; set; }

        public List<string> TextArray { set; get; }

        //https://www.dotnetperls.com/uppercase-first-letter
        // ReSharper disable once MemberCanBePrivate.Global
        [JsonIgnore]
        private readonly string _divider = new string('\uFF3F',1);

        [JsonIgnore]
        public string Text => string.Join(_divider, TextArray);

        public string DisplayText => Text; // in case we want to change the way 
        //cards are displayed when not interpolated

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        public enum Type {
            Call,
            Response
        }

        //this is now purely future-proofing
        public bool NSFW { set; get; }

        [JsonIgnore]
        public int Blanks => TextArray.Count - 1;

        [JsonIgnore]
        public Type CardType => Blanks == 0 ? Type.Response : Type.Call;

        public Card() {
        }

        [JsonConstructor]
        public Card(string id, List<string> text, bool nsfw, string updated_at) {
            TextArray = text;
            ID = id;
            NSFW = nsfw;
        }

        public static string Interpolate(Card call, Card response) {
            var callQ = new Queue<string>(call.TextArray);
            string start = callQ.Dequeue();
            while (callQ.Count != 0) {
                start += response.Text;
                start += callQ.Dequeue();
            }

            return start;
        }

        public bool Equals(Card c) {
            return ID == c.ID;
        }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }

    [Serializable]
    public class Submission {
        private List<Card> _cards;

        [JsonIgnore]
        public List<Card> Cards {
            get => _cards.ToList();
            set => _cards = value;
        }

        public Submission(IEnumerable<Card> cards) {
            _cards = cards.ToList();
        }
    
        [JsonIgnore]
        public string Text => string.Join(" / ", Cards.Select(c => c.Text));
        //yeah, this is kind of unfortunate
    }

}
