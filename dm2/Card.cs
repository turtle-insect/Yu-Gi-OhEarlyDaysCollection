using System.ComponentModel;

namespace dm2
{
    class Card : INotifyPropertyChanged
    {
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;

        public Card(uint address) => mAddress = address;

        public uint ID
        {
            get => SaveData.Instance().ReadNumber(mAddress, 2);
            set
            {
                SaveData.Instance().WriteNumber(mAddress, 2, value);
                SaveData.Instance().WriteNumber(mAddress + 0x0BF8, 2, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
			}
        }
	}
}
