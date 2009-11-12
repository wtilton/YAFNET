﻿using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using YAF.Classes;
using YAF.Classes.Core;
using YAF.Classes.Utils;
using YAF.Classes.Data;

namespace YAF.Pages // YAF.Pages
{
	public partial class im_msn : YAF.Classes.Core.ForumPage
	{
		public int UserID
		{
			get
			{
				return ( int )Security.StringToLongOrRedirect( Request.QueryString ["u"] );
			}
		}

        public im_msn()
			: base( "IM_MSN" )
		{
		}

		protected void Page_Load( object sender, EventArgs e )
		{
			if ( User == null )
			{
				YafBuildLink.AccessDenied();
			}

			if ( !IsPostBack )
			{
				// get user data...
				MembershipUser user = UserMembershipHelper.GetMembershipUserById( UserID );

				if ( user == null )
				{
					YafBuildLink.AccessDenied(/*No such user exists*/);
				}

				PageLinks.AddLink( PageContext.BoardSettings.Name, YafBuildLink.GetLink( ForumPages.forum ) );
				PageLinks.AddLink( user.UserName, YafBuildLink.GetLink( ForumPages.profile, "u={0}", UserID ) );
				PageLinks.AddLink( GetText( "TITLE" ), "" );

				// get full user data...
				CombinedUserDataHelper userData = new CombinedUserDataHelper( user, UserID );

                Msg.NavigateUrl = string.Format("msnim:chat?contact={0}", userData.Profile.MSN);
				//Msg.Attributes.Add( "onclick", "return skypeCheck();" );
                Img.Src = string.Format("http://messenger.services.live.com/users/{0}/presenceimage", userData.Profile.MSN);
			}
		}
	}
}