﻿using SMSBnRTools.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SMSBnRTools
{
    public partial class MainWindow : Form
    {
        private List<contact> contacts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        #region file read

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.Activate();
            string[] files = openFileDialog1.FileNames;

            if (files.Length != 1)
                return;

            string file = files[0];

            filePathInput.Text = file;

            XMLReadResult res = XMLFileHelpers.ReadXml(file);
            if(res.success)
            {
                resultLabel.Text = "File read OK";
                contacts = res.contacts;
                contactsGV.DataSource = contacts;
            }
            else
            {
                resultLabel.Text = "Sorry, cannot read the file.";
            }

            Application.DoEvents();

        }

        #endregion

        #region ui

        private void contactsGV_SelectionChanged(object sender, EventArgs e)
        {
            ResetMessagesGrid();
            if (contactsGV.SelectedRows.Count > 1)
            {
                EnableButtons();
            }
            else if (contactsGV.SelectedRows.Count == 1 && contactsGV.SelectedRows[0].DataBoundItem != null)
            {
                ShowMessagesForRow(contactsGV.SelectedRows[0]);
                EnableButtons();
            }
            else
            {
                DisableButtons();
            }
        }

        private void ShowMessagesForRow(DataGridViewRow row)
        {
            ShowMessages((contact)row.DataBoundItem);
        }

        private void ShowMessages(contact c)
        {
            messagesGV.Enabled = true;
            messagesGV.DataSource = c.messages;
            messagesGV.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        private void messagesGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.Value != null)
            {
                switch ((byte)e.Value)
                {
                    case 1:
                        e.Value = "Received";
                        break;
                    case 2:
                        e.Value = "Sent";
                        break;
                    case 3:
                        e.Value = "Draft";
                        break;
                    case 4:
                        e.Value = "Outbox";
                        break;
                    case 5:
                        e.Value = "Failed";
                        break;
                    case 6:
                        e.Value = "Queued";
                        break;
                    default:
                        e.Value = "";
                        break;
                }
                e.FormattingApplied = true;
            }

        }

        #endregion

        #region buttons
        private void EnableButtons()
        {
            btnDelete.Enabled = true;
            btnMerge.Enabled = contactsGV.SelectedRows.Count > 1;
        }

        private void DisableButtons()
        {
            btnDelete.Enabled = false;
            btnMerge.Enabled = false;
        }

        private void ResetGrid()
        {
            contactsGV.EndEdit();
            contactsGV.DataSource = null;
            contactsGV.DataSource = contacts;
            contactsGV.Refresh();
            contactsGV.CurrentCell = null;
            contactsGV.ClearSelection();
            messagesGV.DataSource = null;
            DisableButtons();
        }

        private void ResetMessagesGrid()
        {
            messagesGV.DataSource = null;
            messagesGV.Enabled = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in contactsGV.SelectedRows)
            {
                contact c = (contact)row.DataBoundItem;
                contacts.Remove(c);
            }
            ResetGrid();
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            if (contactsGV.SelectedRows.Count > 1)
            {
                // first figure out which one is the latest contact (by newest message)
                int latestIndex = -1;
                ulong latestDate = 0;
                for (int i = 0; i < contactsGV.SelectedRows.Count; i++)
                {
                    contact c = (contact)contactsGV.SelectedRows[i].DataBoundItem;
                    if (c.messages.OrderBy(o => o.date).Last().date > latestDate)
                    {
                        latestIndex = i;
                        latestDate = c.messages.OrderBy(o => o.date).Last().date;
                    }
                }

                contact cMaster = (contact)contactsGV.SelectedRows[latestIndex].DataBoundItem;
                for (int i = 0; i < contactsGV.SelectedRows.Count; i++)
                {
                    if (i == latestIndex) // ignore the master contact
                        continue;
                    contact toMerge = (contact)contactsGV.SelectedRows[i].DataBoundItem;
                    cMaster.messages.AddRange(toMerge.messages);
                    // use the number that is associated with a contact_name
                    // TODO: select which contact to merge to?
                    if(cMaster.contact_name == contact.UNKNOWN_NAME && toMerge.contact_name != contact.UNKNOWN_NAME)
                    {
                        cMaster.contact_name = toMerge.contact_name;
                        cMaster.address = toMerge.address;
                    }
                    contacts.Remove(toMerge);
                }
                cMaster.messages = cMaster.messages.OrderBy(o => o.date).ToList();
                ResetGrid();
                // select the new merged contact
                foreach(DataGridViewRow row in contactsGV.Rows)
                {
                    if((contact)row.DataBoundItem == cMaster)
                    {
                        row.Selected = true;
                        ShowMessagesForRow(row);
                        break;
                    }
                }
            }
        }

        private void btnExportSelected_Click(object sender, EventArgs e)
        {
            if (contactsGV.SelectedRows.Count == 1 && contactsGV.SelectedRows[0].DataBoundItem != null)
            {
                var c = (contact)contactsGV.SelectedRows[0].DataBoundItem;
                saveFileDialog1.FileName = c.contact_name != contact.UNKNOWN_NAME ? c.contact_name : (c.contact_name + " (" + c.address + ")");
                saveFileDialog1.ShowDialog();
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.Activate();
            string[] files = saveFileDialog1.FileNames;

            if (files.Length != 1)
                return;

            string file = files[0];

            if (contactsGV.SelectedRows.Count == 1 && contactsGV.SelectedRows[0].DataBoundItem != null)
            {
                SaveXml(file, (contact)contactsGV.SelectedRows[0].DataBoundItem);
            }

            Application.DoEvents();
        }

        private void btnExportAll_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                string folder = folderBrowserDialog1.SelectedPath;
                foreach(contact c in contacts)
                {
                    string filename = c.contact_name != contact.UNKNOWN_NAME ? c.contact_name : (c.contact_name + " (" + c.address + ")");
                    SaveXml(folder + "\\" + filename + ".xml", c);
                }
            }
        }

        private void SaveXml(string filename, contact c)
        {
            // Create an instance of the class to be serialized.
            smses s = new smses();
            s.Items = c.messages.ToArray();

            // handle existing files - check for duplicate messages, append new
            if (File.Exists(filename))
            {
                FileExistsDialog feg = new FileExistsDialog("This file already exists!\nDo you want to merge the messages, overwrite the file or cancel?\n" + filename);
                feg.Owner = this;
                feg.StartPosition = FormStartPosition.CenterParent;
                DialogResult result = feg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    XMLReadResult readResult = XMLFileHelpers.ReadXml(filename);
                    if (!readResult.success)
                    {
                        MessageBox.Show("Error reading file", "The file could not be read as a valid SMS Backup & Restore XML file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (readResult.contacts.Any())
                    {
                        List<IMobileMessage> merged = c.messages;
                        foreach (var existingContact in readResult.contacts)
                            merged.AddRange(existingContact.messages);

                        List<IMobileMessage> removedDupes = merged.GroupBy(g => new { g.date, g.sentReceived, g.bodyText }).Select(x => x.First()).OrderBy(o => o.date).ToList();
                        s.Items = removedDupes.ToArray();
                    }
                    else // valid file, but no messages/contacts
                    {
                        //s.Items = c.smses.ToArray();
                    }

                }
                else if (result == System.Windows.Forms.DialogResult.No)
                {
                    //s.Items = c.smses.ToArray();
                }
                else
                {
                    return;
                }
            }
            s.count = (ushort)s.Items.Count();
            XMLFileHelpers.SaveXml(filename, s);
        }

        #endregion
    }
}
