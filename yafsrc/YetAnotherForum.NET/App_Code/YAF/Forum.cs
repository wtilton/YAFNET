/* YetAnotherForum.NET
 * Copyright (C) 2006-2007 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Web;
using System.Web.UI;
using YAF.Classes.Utils;
using YAF.Classes.Base;

namespace YAF
{
	/// <summary>
	/// Summary description for Forum.
	/// </summary>
	[ToolboxData( "<{0}:Forum runat=\"server\"></{0}:Forum>" )]
	public class Forum : System.Web.UI.UserControl
	{
		YafControlSettings forumSettings = new YafControlSettings();
		private YAF.Controls.Header m_header = new YAF.Controls.Header();
		private YAF.Controls.Footer m_footer = new YAF.Controls.Footer();
		public event EventHandler<YAF.Classes.Base.ForumPageArgs> PageTitleSet;

		public Forum()
		{
			YAF.Classes.Utils.YafContext.Current.Settings = forumSettings;
			this.Load += new EventHandler( Forum_Load );
		}

		private void Forum_Load( object sender, EventArgs e )
		{
			YAF.Classes.Utils.ForumPages Page;
			string m_baseDir = YafForumInfo.ForumRoot;

			try
			{
				Page = ( YAF.Classes.Utils.ForumPages ) System.Enum.Parse( typeof( YAF.Classes.Utils.ForumPages ), Request.QueryString ["g"], true );
			}
			catch ( Exception )
			{
				Page = YAF.Classes.Utils.ForumPages.forum;
			}

			if ( !ValidPage( Page ) )
			{
				YAF.Classes.Utils.YafBuildLink.Redirect( YAF.Classes.Utils.ForumPages.topics, "f={0}", LockedForum );
			}

			string src = string.Format( "{0}pages/{1}.ascx", m_baseDir, Page );
			if ( src.IndexOf( "/moderate_" ) >= 0 )
				src = src.Replace( "/moderate_", "/moderate/" );
			if ( src.IndexOf( "/admin_" ) >= 0 )
				src = src.Replace( "/admin_", "/admin/" );
			if ( src.IndexOf( "/help_" ) >= 0 )
				src = src.Replace( "/help_", "/help/" );

			try
			{
				YAF.Classes.Base.ForumPage forumControl = ( YAF.Classes.Base.ForumPage ) LoadControl( src );
				forumControl.ForumFooter = m_footer;
				forumControl.ForumHeader = m_header;
				forumControl.PageTitleSet += new EventHandler<YAF.Classes.Base.ForumPageArgs>( forumControl_PageTitleSet );
				
				// add the header control before the page rendering...
				if ( YafContext.Current.Settings.LockedForum == 0 )
					this.Controls.AddAt( 0, m_header );

				this.Controls.Add( forumControl );

				// add the footer control after the page...
				if ( YafContext.Current.Settings.LockedForum == 0 )
					this.Controls.Add( m_footer );
			}
			catch ( System.IO.FileNotFoundException )
			{
				throw new ApplicationException( "Failed to load " + src + "." );
			}
		}
		
		/// <summary>
		/// Called when the forum control sets it's Page Title
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
    void forumControl_PageTitleSet( object sender, YAF.Classes.Base.ForumPageArgs e )
    {
      if ( PageTitleSet != null ) PageTitleSet( this, e );
    }		

		/// <summary>
		/// The forum header control
		/// </summary>
		public YAF.Controls.Header Header
		{
			set
			{
				m_header = value;
			}
			get
			{
				return m_header;
			}
		}

		/// <summary>
		/// The forum footer control
		/// </summary>
		public YAF.Controls.Footer Footer
		{
			set
			{
				m_footer = value;
			}
			get
			{
				return m_footer;
			}
		}

		private bool ValidPage( ForumPages Page )
		{
			if ( LockedForum == 0 )
				return true;

			if ( Page == YAF.Classes.Utils.ForumPages.forum || Page == YAF.Classes.Utils.ForumPages.active || Page == YAF.Classes.Utils.ForumPages.activeusers )
				return false;

			if ( Page == YAF.Classes.Utils.ForumPages.cp_editprofile || Page == YAF.Classes.Utils.ForumPages.cp_pm || Page == YAF.Classes.Utils.ForumPages.cp_message || Page == YAF.Classes.Utils.ForumPages.cp_profile || Page == YAF.Classes.Utils.ForumPages.cp_signature || Page == YAF.Classes.Utils.ForumPages.cp_subscriptions )
				return false;

			if ( Page == YAF.Classes.Utils.ForumPages.pmessage )
				return false;

			return true;
		}

		/// <summary>
		/// UserID for the current User (Read Only)
		/// </summary>
		public int PageUserID
		{
			get
			{
				return YafContext.Current.PageUserID;
			}
		}

		/// <summary>
		/// UserName for the current User (Read Only)
		/// </summary>
		public string PageUserName
		{
			get
			{
				if ( YafContext.Current.User == null ) return "Guest";				
				return YafContext.Current.User.UserName;
			}
		}

		/// <summary>
		/// Get or sets the Board ID for this instance of the forum control
		/// </summary>
		public int BoardID
		{
			get
			{
				return forumSettings.BoardID;
			}
			set
			{
				forumSettings.BoardID = value;
			}
		}

		/// <summary>
		/// Gets or sets the CategoryID for this instance of the forum control
		/// </summary>
		public int CategoryID
		{
			get
			{
				return forumSettings.CategoryID;
			}
			set
			{
				forumSettings.CategoryID = value;
			}
		}

		public int LockedForum
		{
			get
			{
				return forumSettings.LockedForum;
			}
			set
			{
				forumSettings.LockedForum = value;
			}
		}
	}
}
