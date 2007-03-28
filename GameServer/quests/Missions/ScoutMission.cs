using System;
using System.Collections;

using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
	public class ScoutMission : AbstractMission
	{
		private AbstractGameKeep m_keep = null;

		public ScoutMission(object owner)
			: base(owner)
		{
			int realm = 0;
			if (owner is PlayerGroup)
				realm = (owner as PlayerGroup).Leader.Realm;
			else if (owner is GamePlayer)
				realm = (owner as GamePlayer).Realm;

			ArrayList list = new ArrayList();

			IList keeps;
			if (owner is PlayerGroup)
				keeps = KeepMgr.GetKeepsOfRegion((owner as PlayerGroup).Leader.CurrentRegionID);
			else if (owner is GamePlayer)
				keeps = KeepMgr.GetKeepsOfRegion((owner as GamePlayer).CurrentRegionID);
			else keeps = new ArrayList();

			foreach (AbstractGameKeep keep in keeps)
			{
				if (keep.IsPortalKeep)
					continue;
				if (keep.Realm != realm)
					list.Add(keep);
			}

			if (list.Count > 0)
				m_keep = list[Util.Random(list.Count - 1)] as AbstractGameKeep;

			GameEventMgr.AddHandler(AreaEvent.PlayerEnter, new DOLEventHandler(Notify));
			GameEventMgr.AddHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
		}

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e == AreaEvent.PlayerEnter)
			{
				AreaEventArgs kargs = args as AreaEventArgs;

				if (m_owner is GamePlayer && kargs.GameObject != m_owner)
					return;

				foreach (AbstractArea area in kargs.GameObject.CurrentAreas)
				{
					if (area is KeepArea && (area as KeepArea).Keep == m_keep)
					{
						FinishMission();
						break;
					}
				}
			}
			else if (e == KeepEvent.KeepTaken)
			{
				KeepEventArgs kargs = args as KeepEventArgs;

				if (kargs.Keep != m_keep)
					return;

				ExpireMission();
			}
		}

		public override void FinishMission()
		{
			base.FinishMission();
			GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(Notify));
			GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
		}

		public override void ExpireMission()
		{
			base.ExpireMission();
			GameEventMgr.RemoveHandler(AreaEvent.PlayerEnter, new DOLEventHandler(Notify));
			GameEventMgr.RemoveHandler(KeepEvent.KeepTaken, new DOLEventHandler(Notify));
		}

		public override string Description
		{
			get
			{
				if (m_keep == null)
					return "Keep is null when trying to send the description";
				else return "Scout the area around " + m_keep.Name;
			}
		}
	}
}