using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace dm2
{
    class ViewModel
    {
        public ICommand OpenFileCommand { get; init; }
		public ICommand SaveFileCommand { get; init; }
		public ICommand ChoiceCardCommand { get; init; }
		public ICommand ChangeAllCardCommand { get; init; }

		public Info Info { get; init; } = Info.Instance();
		public ObservableCollection<Card> Deck { get; init; } = new ObservableCollection<Card>();

        public int SelectCardIndex { get; set; } = 0;

        public ViewModel()
        {
			OpenFileCommand = new ActionCommand(OpenFile);
			SaveFileCommand = new ActionCommand(SaveFile);
			ChoiceCardCommand = new ActionCommand(ChoiceCard);
			ChangeAllCardCommand = new ActionCommand(ChangeAllCard);
		}

		public void Initialize()
        {
            Deck.Clear();
            for (uint i = 0; i < 40; i++)
            {
                var card = new Card(0x0B + i * 2);
                Deck.Add(card);
            }
		}

        private void OpenFile(Object? parameter)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "save|*.*sav";
            if (dlg.ShowDialog() == false) return;

            if (SaveData.Instance().Open(dlg.FileName, false) == true)
            {
                MessageBox.Show("OK");
                Initialize();
            }
        }

        private void SaveFile(Object? parameter)
        {
            SaveData.Instance().Save();
        }

        private void ChoiceCard(Object? parameter)
        {
			if (parameter is not Card card) return;

			var dlg = new ChoiceWindow();
			dlg.ID = card.ID;
			dlg.ShowDialog();

			card.ID = dlg.ID;
		}

		private void ChangeAllCard(Object? parameter)
		{
            foreach (var card in Deck)
            {
                card.ID = Info.Card[SelectCardIndex].Value;
            }
		}
	}
}
