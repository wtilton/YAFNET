﻿/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjّrnar Henden
 * Copyright (C) 2006-2010 Jaben Cargman
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

namespace YAF.Pages
{
    using System;
    using System.Data;
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using YAF.Classes;
    using YAF.Classes.Core;
    using YAF.Classes.Data;
    using YAF.Classes.Utils;

    /// <summary>
    /// The cp_editalbumimages.
    /// </summary>
    public partial class cp_editalbumimages : ForumPageRegistered
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the cp_editalbumimages class.
        /// </summary>
        public cp_editalbumimages()
            : base("CP_EDITALBUMIMAGES")
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// the page load event.
        /// </summary>
        /// <param name="sender">
        /// the sender.
        /// </param>
        /// <param name="e">
        /// the e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!this.PageContext.BoardSettings.EnableAlbum)
            {
                YafBuildLink.AccessDenied();
            }

            if (!this.IsPostBack)
            {
                int[] albumSize = DB.album_getstats(this.PageContext.PageUserID, null);
                int userID;
                switch (this.Request.QueryString["a"])
                {
                    // A new album is being created. check the permissions.
                    case "new":

                        // Is album feature enabled?
                        if (!this.PageContext.BoardSettings.EnableAlbum)
                        {
                            YafBuildLink.AccessDenied();
                        }

                        // Has the user created maximum number of albums?
                        if (this.PageContext.BoardSettings.AlbumsMax > 0 &&
                            albumSize[0] > this.PageContext.BoardSettings.AlbumsMax - 1)
                        {
                            YafBuildLink.RedirectInfoPage(InfoMessage.AccessDenied);
                        }

                        userID = this.PageContext.PageUserID;
                        break;
                    default:
                        userID =
                            Convert.ToInt32(
                                DB.album_list(
                                    null, Security.StringToLongOrRedirect(this.Request.QueryString["a"].ToString())).
                                    Rows[0]["UserID"]);
                        if (userID != this.PageContext.PageUserID)
                        {
                            YafBuildLink.AccessDenied();
                        }

                        break;
                }

                // Has the user uploaded maximum number of images? 
                if (this.PageContext.BoardSettings.AlbumImagesNumberMax > 0)
                {
                    if (albumSize[1] > this.PageContext.BoardSettings.AlbumImagesNumberMax - 1)
                    {
                        this.uploadtitletr.Visible = false;
                    }
                }

                // Add the page links.
                this.PageLinks.AddLink(this.PageContext.BoardSettings.Name, YafBuildLink.GetLink(ForumPages.forum));
                this.PageLinks.AddLink(
                    UserMembershipHelper.GetUserNameFromID(userID),
                    YafBuildLink.GetLink(ForumPages.profile, "u={0}", userID.ToString()));
                this.PageLinks.AddLink(
                    this.GetText("ALBUMS"), YafBuildLink.GetLink(ForumPages.albums, "u={0}", userID.ToString()));
                this.PageLinks.AddLink(this.GetText("TITLE"), string.Empty);

                this.Back.Text = this.GetText("BACK");
                this.Upload.Text = this.GetText("UPLOAD");

                this.BindData();
            }
        }

        /// <summary>
        /// The back button click event handler.
        /// </summary>
        /// <param name="sender">
        /// the sender.
        /// </param>
        /// <param name="e">
        /// the e.
        /// </param>
        protected void Back_Click(object sender, EventArgs e)
        {
            if (this.List.Items.Count > 0)
            {
                YafBuildLink.Redirect(
                    ForumPages.album, 
                    "u={0}&a={1}", 
                    this.PageContext.PageUserID.ToString(), 
                    this.Request.QueryString["a"]);
            }
            else
            {
                YafBuildLink.Redirect(ForumPages.albums, "u={0}", this.PageContext.PageUserID);
            }
        }

        /// <summary>
        /// Deletes the album and all the images in it.
        /// </summary>
        /// <param name="sender">
        /// the sender.
        /// </param>
        /// <param name="e">
        /// the e.
        /// </param>
        protected void DeleteAlbum_Click(object sender, EventArgs e)
        {
            string sUpDir = this.Request.MapPath(String.Concat(UrlBuilder.FileRoot, YafBoardFolders.Current.Uploads));
            YafAlbum.Album_Image_Delete(sUpDir, this.Request.QueryString["a"], this.PageContext.PageUserID, null);
            YafBuildLink.Redirect(ForumPages.albums, "u={0}", this.PageContext.PageUserID);
        }

        /// <summary>
        /// The btn delete_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void DeleteAlbum_Load(object sender, EventArgs e)
        {
            ((Button)sender).Attributes["onclick"] = string.Format(
                "return confirm(\'{0}\')", this.GetText("ASK_DELETEALBUM"));
        }

        /// <summary>
        /// Update the album title.
        /// </summary>
        /// <param name="sender">
        /// the sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void UpdateTitle_Click(object sender, EventArgs e)
        {
            string albumID = this.Request.QueryString["a"];
            if (this.Request.QueryString["a"] == "new")
            {
                albumID = DB.album_save(null, this.PageContext.PageUserID, this.txtTitle.Text, null).ToString();
            }
            else
            {
                DB.album_save(this.Request.QueryString["a"], null, this.txtTitle.Text, null);
            }

            YafBuildLink.Redirect(ForumPages.cp_editalbumimages, "a={0}", albumID);
        }

        /// <summary>
        /// The Upload file delete confirmation dialog.
        /// </summary>
        /// <param name="sender">
        /// the sender.
        /// </param>
        /// <param name="e">
        /// the e.
        /// </param>
        protected void ImageDelete_Load(object sender, EventArgs e)
        {
            ((LinkButton)sender).Attributes["onclick"] = String.Format(
                "return confirm('{0}')", this.GetText("ASK_DELETEIMAGE"));
        }

        /// <summary>
        /// The repater Item command event responsible for handling deletion of uploaded files.
        /// </summary>
        /// <param name="source">
        /// the source.
        /// </param>
        /// <param name="e">
        /// the e.
        /// </param>
        protected void List_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "delete":
                    string sUpDir =
                        this.Request.MapPath(String.Concat(UrlBuilder.FileRoot, YafBoardFolders.Current.Uploads));
                    YafAlbum.Album_Image_Delete(
                        sUpDir, null, this.PageContext.PageUserID, Convert.ToInt32(e.CommandArgument));
                    this.BindData();
                    this.uploadtitletr.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// The Upload button click event handler.
        /// </summary>
        /// <param name="sender">
        /// the sender.
        /// </param>
        /// <param name="e">
        /// the e.
        /// </param>
        protected void Upload_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.CheckValidFile(this.File))
                {
                    this.SaveAttachment(this.File);
                }

                this.BindData();
            }
            catch (Exception x)
            {
                DB.eventlog_create(this.PageContext.PageUserID, this, x);
                this.PageContext.AddLoadMessage(x.Message);
                return;
            }
        }

        /// <summary>
        /// Initializes the repeater control and the visibilities of form elements.
        /// </summary>
        private void BindData()
        {
            // If the user is trying to edit an existing album, initialize the repeater.
            if (this.Request.QueryString["a"] != "new")
            {
                this.txtTitle.Text = DB.album_gettitle(this.Request.QueryString["a"]);
                DataTable dt = DB.album_image_list(this.Request.QueryString["a"], null);
                this.List.DataSource = dt;
                this.List.Visible = (dt.Rows.Count > 0) ? true : false;
                this.Delete.Visible = true;
                this.DataBind();
            }
            else
            {
                this.Delete.Visible = false;
            }
        }

        /// <summary>
        /// Check to see if the user is trying to upload a valid file.
        /// </summary>
        /// <param name="uploadedFile">
        /// the uploaded file.
        /// </param>
        /// <returns>
        /// true if file is valid for uploading. otherwise false.
        /// </returns>
        private bool CheckValidFile(HtmlInputFile uploadedFile)
        {
            string filePath = uploadedFile.PostedFile.FileName.Trim();

            if (String.IsNullOrEmpty(filePath) || uploadedFile.PostedFile.ContentLength == 0)
            {
                return false;
            }

            string extension = Path.GetExtension(filePath).ToLower();

            // remove the "period"
            extension = extension.Replace(".", string.Empty);
            string[] aImageExtensions = { "jpg", "gif", "png", "bmp" };

            // If we don't get a match from the db, then the extension is not allowed
            DataTable dt = DB.extension_list(this.PageContext.PageBoardID, extension);

            // also, check to see an image is being uploaded.
            if (Array.IndexOf(aImageExtensions, extension) == -1 || dt.Rows.Count == 0)
            {
                this.PageContext.AddLoadMessage(this.GetTextFormatted("FILEERROR", extension));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save the attached file both physically and in the db.
        /// </summary>
        /// <param name="file">
        /// the file.
        /// </param>
        private void SaveAttachment(HtmlInputFile file)
        {
            if (file.PostedFile == null || file.PostedFile.FileName.Trim().Length == 0 ||
                file.PostedFile.ContentLength == 0)
            {
                return;
            }

            string sUpDir = this.Request.MapPath(String.Concat(UrlBuilder.FileRoot, YafBoardFolders.Current.Uploads));
            string filename = file.PostedFile.FileName;

            int pos = filename.LastIndexOfAny(new[] { '/', '\\' });
            if (pos >= 0)
            {
                filename = filename.Substring(pos + 1);
            }

            // filename can be only 255 characters long (due to table column)
            if (filename.Length > 255)
            {
                filename = filename.Substring(filename.Length - 255);
            }

            // verify the size of the attachment
            if (this.PageContext.BoardSettings.AlbumImagesSizeMax > 0 &&
                file.PostedFile.ContentLength > this.PageContext.BoardSettings.AlbumImagesSizeMax)
            {
                throw new Exception(this.GetText("ERROR_TOOBIG"));
            }

            if (this.Request.QueryString["a"] == "new")
            {
                int albumID = DB.album_save(null, this.PageContext.PageUserID, this.txtTitle.Text, null);
                file.PostedFile.SaveAs(
                    String.Format(
                        "{0}/{1}.{2}.{3}.yafalbum", sUpDir, this.PageContext.PageUserID, albumID.ToString(), filename));
                DB.album_image_save(
                    null, albumID, null, filename, file.PostedFile.ContentLength, file.PostedFile.ContentType);
                YafBuildLink.Redirect(ForumPages.cp_editalbumimages, "a={0}", albumID);
            }
            else
            {
                file.PostedFile.SaveAs(
                    String.Format(
                        "{0}/{1}.{2}.{3}.yafalbum", 
                        sUpDir, 
                        this.PageContext.PageUserID, 
                        this.Request.QueryString["a"], 
                        filename));
                DB.album_image_save(
                    null, 
                    this.Request.QueryString["a"], 
                    null, 
                    filename, 
                    file.PostedFile.ContentLength, 
                    file.PostedFile.ContentType);
            }
        }

        #endregion
    }
}