using System;
using System.Collections.Generic;
using System.Text;

namespace ASCTracInterfaceDll.Imports
{
    internal class ImportNotes
    {
        internal static string SaveNotes( string aType, string aOrderNum, string aNote,  bool aBaseOrder, int aLineNum, int aSeq, ParseNet.GlobalClass Globals)
		{
			string errmsg = string.Empty;
			if ( !String.IsNullOrEmpty( aNote) &&			( aNote.Trim() != ""))
			{
				string sqlStr, newNote = aNote.Replace("'", "''");
				bool notesRecExists;

				if (newNote.Length > 250) newNote = newNote.Substring(0, 250);  //added 12-18-19 (JXG)

				sqlStr = "SELECT ORDERNUM FROM NOTES (NOLOCK)" +
					" WHERE ORDERNUM='" + aOrderNum + "'" +
					" AND TYPE='" + aType + "'" +
					" AND LINENUM=" + aLineNum.ToString() +
					" AND SEQNUM=" + aSeq.ToString();

				notesRecExists = Globals.myDBUtils.ifRecExists(sqlStr);

				if (aBaseOrder)
					notesRecExists = true;

				if (notesRecExists)
				{
					if (aBaseOrder)
						sqlStr = "UPDATE NOTES SET NOTE='" + newNote + "'" +
							" WHERE SUBSTRING(ORDERNUM,1," + aOrderNum.Length.ToString() + ")='" + aOrderNum + "'" +
							" AND TYPE='" + aType + "'" +
							" AND LINENUM=" + aLineNum.ToString() +
							" AND SEQNUM=" + aSeq.ToString();
					else
						sqlStr = "UPDATE NOTES SET NOTE='" + newNote + "'" +
							" WHERE ORDERNUM='" + aOrderNum + "'" +
							" AND TYPE='" + aType + "'" +
							" AND LINENUM=" + aLineNum.ToString() +
							" AND SEQNUM=" + aSeq.ToString();

					Globals.mydmupdate.AddToUpdate(sqlStr);
				}
				else
				{
					// have to insert this way to avoid the 'comma in first char' problem
					sqlStr = "INSERT INTO NOTES (ORDERNUM, LINENUM, SEQNUM, TYPE, NOTE) VALUES (" +
						"'" + aOrderNum + "'," +
						aLineNum.ToString() + "," +
						aSeq.ToString() + "," +
						"'" + aType + "'," +
						"'" + newNote + "')";
					Globals.mydmupdate.AddToUpdate(sqlStr);
				}
			}
			return (errmsg);
		}

	}
}
