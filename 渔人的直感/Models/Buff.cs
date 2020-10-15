using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 渔人的直感.Models
{
	/// <summary>
	/// 玩家身上的buff状态
	/// </summary>
	public class Buff : INotifyPropertyChanged
	{
		/// <summary>
		/// BuffID.
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Buff层数.
		/// </summary>
		public int Stacks { get; set; }

		/// <summary>
		/// Buff剩余时间.
		/// </summary>
		public float Duration { get; set; }

		/// <summary>
		/// Buff的主人.
		/// </summary>
		public int Owner { get; set; }

		/*public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(Waymark))
				return base.Equals(obj);
			var w = obj as Waymark;
			return w.X == X && w.Y == Y && w.Z == Z && w.Active == Active && w.ID == ID;
		}

		public override int GetHashCode() => X.GetHashCode() & Y.GetHashCode() & Z.GetHashCode() & ID.GetHashCode() & Active.GetHashCode();*/

		/// <summary>
		/// PropertyChanged event handler for this model.
		/// </summary>
#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
	}
}
