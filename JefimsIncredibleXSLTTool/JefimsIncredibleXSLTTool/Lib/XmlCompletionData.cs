using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace JefimsIncredibleXsltTool.Lib
{
    /// <inheritdoc />
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    public class XmlCompletionData : ICompletionData
    {
        public List<string> Quotes = new List<string>
        {
            "'Any idiot can face a crisis - it's day to day living that wears you out.' - Anton Chekhov",
            "'If you are afraid of loneliness, don't marry.' - Anton Chekhov",
            "'Medicine is my lawful wife and literature my mistress; when I get tired of one, I spend the night with the other.' - Anton Chekhov",
            "'Don't tell me the moon is shining; show me the glint of light on broken glass.' - Anton Chekhov",
            "'Doctors are the same as lawyers; the only difference is that lawyers merely rob you, whereas doctors rob you and kill you too.' - Anton Chekhov",
            "'We shall find peace. We shall hear the angels, we shall see the sky sparkling with diamonds.' - Anton Chekhov",
            "'Knowledge is of no value unless you put it into practice.' - Anton Chekhov",
            "'Money, like vodka, turns a person into an eccentric.' - Anton Chekhov",
            "'Love, friendship and respect do not unite people as much as a common hatred for something.' - Anton Chekhov",
            "'You must trust and believe in people or life becomes impossible.' - Anton Chekhov",
            "'The soul is healed by being with children.' - Fyodor Dostoyevsky",
            "'The second half of a man's life is made up of nothing but the habits he has acquired during the first half.' - Fyodor Dostoyevsky",
            "'Beauty is mysterious as well as terrible.God and devil are fighting there, and the battlefield is the heart of man.' - Fyodor Dostoyevsky",
            "'Taking a new step, uttering a new word, is what people fear most.' - Fyodor Dostoyevsky",
            "'What is hell? I maintain that it is the suffering of being unable to love.' - Fyodor Dostoyevsky",
            "'If there is no God, everything is permitted.' - Fyodor Dostoyevsky",
            "'The greatest happiness is to know the source of unhappiness.' - Fyodor Dostoyevsky",
            "'To live without Hope is to Cease to live.' - Fyodor Dostoyevsky",
            "'Much unhappiness has come into the world because of bewilderment and things left unsaid.' - Fyodor Dostoyevsky",
            "'Man only likes to count his troubles, but he does not count his joys.' - Fyodor Dostoyevsky",
            "'Everyone thinks of changing the world, but no one thinks of changing himself.' - Leo Tolstoy",
            "'If you want to be happy, be.' - Leo Tolstoy",
            "'Happy families are all alike; every unhappy family is unhappy in its own way.' - Leo Tolstoy",
            "'All, everything that I understand, I understand only because I love.' - Leo Tolstoy",
            "'The two most powerful warriors are patience and time.' - Leo Tolstoy",
            "'There is no greatness where there is no simplicity, goodness and truth.' - Leo Tolstoy",
            "'Truth, like gold, is to be obtained not by its growth, but by washing away from it all that is not gold.' - Leo Tolstoy",
            "'In the name of God, stop a moment, cease your work, look around you.' - Leo Tolstoy",
            "'We can know only that we know nothing. And that is the highest degree of human wisdom.' - Leo Tolstoy",
            "'The sole meaning of life is to serve humanity.' - Leo Tolstoy",
            "'Be not afraid of greatness: some are born great, some achieve greatness, and some have greatness thrust upon them.' - William Shakespeare",
            "'To thine own self be true, and it must follow, as the night the day, thou canst not then be false to any man.' - William Shakespeare",
            "'The course of true love never did run smooth.' - William Shakespeare",
            "'There is nothing either good or bad, but thinking makes it so.' - William Shakespeare",
            "'Love looks not with the eyes, but with the mind; and therefore is winged Cupid painted blind.' - William Shakespeare",
            "'Cowards die many times before their deaths; the valiant never taste of death but once.' - William Shakespeare",
            "'If music be the food of love, play on.' - William Shakespeare",
            "'Love all, trust a few, do wrong to none.' - William Shakespeare",
            "'Life... is a tale Told by an idiot, full of sound and fury, Signifying nothing.' - William Shakespeare",
            "'Men at some time are the masters of their fates: The fault, dear Brutus, is not in our stars, but in ourselves, that we are underlings.' - William Shakespeare",
            "'Imagination is more important than knowledge.' - Albert Einstein",
            "'The important thing is not to stop questioning. Curiosity has its own reason for existing.' - Albert Einstein",
            "'Anyone who has never made a mistake has never tried anything new.' - Albert Einstein",
            "'Try not to become a man of success, but rather try to become a man of value.' - Albert Einstein",
            "'Two things are infinite: the universe and human stupidity; and I'm not sure about the universe.' - Albert Einstein",
            "'Science without religion is lame, religion without science is blind.' - Albert Einstein",
            "'The significant problems we have cannot be solved at the same level of thinking with which we created them.' - Albert Einstein",
            "'Everything should be made as simple as possible, but not simpler.' - Albert Einstein",
            "'The most beautiful thing we can experience is the mysterious.It is the source of all true art and science.' - Albert Einstein",
            "'I have no special talents.I am only passionately curious.' - Albert Einstein",
            "'Pay no attention to what the critics say; no statue has ever been erected to a critic.' - Jean Sibelius",
            "'A strong desire derives a person straight through the hardest rock.' - Aleksis Kivi",
            "'The love goes where it wants; you hear it rustling but you do not know where it's going and where it goes.' - Aleksis Kivi",
            "'I only want to live in peace, plant potatoes and dream!' - Tove Jansson",
            "'You can't ever be really free if you admire somebody too much.' - Tove Jansson",
            "'Maybe my passion is nothing special, but at least it's mine.' - Tove Jansson"
        };

        public XmlCompletionData(string content, string text, string description)
        {
            Content = content;
            Text = text;
            Description = description;
        }

        public ImageSource Image => null;

        public string Text { get; }

        public object Content { get; }
        public object Description { get; }

        public double Priority => 1;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}
