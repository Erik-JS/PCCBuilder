using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PCCBuilder
{
    class PCC
    {

        public byte[] RawContents;

        public struct FileHeader
        {
            public int MagicNumber; // 0x0
            public int Version; // 0x4
            public int HeaderSize; // 0x8
            public int NoneCharCount; // 0xC
            public string None; // 0x10
            public int PackageFlags; // 0x1A
            public int Int1E; // 0x1E
            public int NameCount; // 0x22
            public int NamePointer; // 0x26
            public int ExportCount; // 0x2A
            public int ExportPointer; // 0x2E
            public int ImportCount; // 0x32
            public int ImportPointer; // 0x36
            public int HeaderSize1; // 0x3A
            public int HeaderSize2; // 0x3E
            public int Int42; // 0x42
            public int Int46; // 0x46
            public int Int4A; // 0x4A
            public byte[] GUID; // 0x4E
            public int Generations; // 0x5E
            public int Unk62; // 0x62
            public int Unk66; // 0x66
            public int Unk6A; // 0x6A
            public int Unk6E; // 0x6E
            public int Unk72; // 0x72
            public int GameVersion; // 0x76
            public int Unk7A; // 0x7A
            public int Unk7E; // 0x7E
            public int Unk82; // 0x82
            public int Unk86; // 0x86
            public int Unk8A; // 0x8A
        }

        public FileHeader header;

        public string[] Names;

        public struct ImportEntry
        {
            public int PackageIndex; // 0x0
            public int Unknown1; // 0x4
            public int ObjTypeIndex; // 0x8
            public int Unknown2; // 0xC
            public int OwnerRef; // 0x10
            public int NameIndex; // 0x14
            public int Unknown3; // 0x18
        }

        public ImportEntry[] Imports;

        public struct ExportEntry
        {
            public int ObjTypeRef; // 0x0
            public int ParentClassRef; // 0x4
            public int OwnerRef; // 0x8
            public int NameIndex; // 0xC
            public int NameCount; // 0x10
            public int Unk14; // 0x14
            public int Unk18; // 0x18
            public int Int1C; // 0x1C
            public int DataSize; // 0x20
            public int DataOffset; // 0x24
            public int Unk28; // 0x28
            public int ExtraFieldsCount; // 0x2C
            public byte[] GUID; // 0x30
            public int Flags; // 0x40
            public int[] Extra; // 0x44
        }

        public ExportEntry[] Exports;

        public string[] ObjectDataFiles;

        public List<byte> ObjectData = new List<byte>();

        public string FolderPath;

        public string ObjectFolder;

        public int Gap = 0;

        public static PCC LoadFromFile(string filename)
        {
            try
            {
                Form1.Log("Loading file: " + Path.GetFileName(filename));
                PCC res = new PCC();
                res.FolderPath = Path.GetDirectoryName(filename);
                res.ObjectFolder = Path.GetFileNameWithoutExtension(filename);
                res.RawContents = File.ReadAllBytes(filename);
                Form1.Log("Size: " + res.RawContents.Length + " bytes", Color.LightBlue);
                //int x = res.ReadInt32(0x8);
                //byte[] temparray = new byte[x];
                //Array.Copy(res.RawContents, 0, temparray, 0, x);
                //res.RawContents = temparray;
                res.FileHeaderOperation();
                res.NameTableOperation();
                Form1.Log("Names: " + res.Names.Length, Color.LightBlue);
                res.ImportsOperation();
                Form1.Log("Imports: " + res.Imports.Length, Color.LightBlue);
                res.ExportsOperation();
                Form1.Log("Exports: " + res.Exports.Length, Color.LightBlue);
                res.ObjDataOperation();
                return res;
            }
            catch (Exception ex)
            {
                Form1.Log("LoadFromFile | " + ex.GetType().Name + ": " + ex.Message, Form1.errorColor);
                return null;
            }
        }

        public int ReadInt32(int index)
        {
            int intValue = RawContents[index];
            intValue += (int)RawContents[index + 1] << 8;
            intValue += (int)RawContents[index + 2] << 16;
            intValue += (int)RawContents[index + 3] << 24;
            return intValue;
        }

        public short ReadInt16(int index)
        {
            short shortValue = RawContents[index];
            shortValue += (short)(RawContents[index + 1] << 8);
            return shortValue;
        }

        public string ReadUnicodeString(int index)
        {
            List<char> listChar = new List<char>();
            short charvalue;
            while ( (charvalue = ReadInt16(index)) != 0 )
            {
                listChar.Add((char)charvalue);
                index += 2;
            }
            return new string(listChar.ToArray());
        }

        public byte[] ReadByteArray(int index, int size)
        {
            byte[] res = new byte[size];
            Array.Copy(RawContents, index, res, 0, size);
            return res;
        }

        /// <summary>
        /// Reads all values for header.
        /// </summary>
        private void FileHeaderOperation()
        {
            header.MagicNumber = ReadInt32(0);
            header.Version = ReadInt32(4);
            header.HeaderSize = ReadInt32(8);
            header.NoneCharCount = ReadInt32(0xC) * -1;
            header.None = ReadUnicodeString(0x10);
            header.PackageFlags = ReadInt32(0x1A);
            header.Int1E = ReadInt32(0x1E);
            header.NameCount = ReadInt32(0x22);
            header.NamePointer = ReadInt32(0x26);
            header.ExportCount = ReadInt32(0x2A);
            header.ExportPointer = ReadInt32(0x2E);
            header.ImportCount = ReadInt32(0x32);
            header.ImportPointer = ReadInt32(0x36);
            header.HeaderSize1 = ReadInt32(0x3A);
            header.HeaderSize2 = ReadInt32(0x3E);
            header.Int42 = ReadInt32(0x42);
            header.Int46 = ReadInt32(0x46);
            header.Int4A = ReadInt32(0x4A);
            header.GUID = ReadByteArray(0x4E, 0x10);
            header.Generations = ReadInt32(0x5E);
            header.Unk62 = ReadInt32(0x62);
            header.Unk66 = ReadInt32(0x66);
            header.Unk6A = ReadInt32(0x6A);
            header.Unk6E = ReadInt32(0x6E);
            header.Unk72 = ReadInt32(0x72);
            header.GameVersion = ReadInt32(0x76);
            header.Unk7A = ReadInt32(0x7A);
            header.Unk7E = ReadInt32(0x7E);
            header.Unk82 = ReadInt32(0x82);
            header.Unk86 = ReadInt32(0x86);
            header.Unk8A = ReadInt32(0x8A);
        }

        /// <summary>
        /// Reads all names in the name table.
        /// </summary>
        private void NameTableOperation()
        {
            Names = new string[header.NameCount];
            int offset = 0;
            for (int i = 0; i < Names.Length; i++)
            {
                int charcount = ReadInt32(header.NamePointer + offset) * -1;
                offset += 4;
                Names[i] = ReadUnicodeString(header.NamePointer + offset);
                offset += charcount * 2;
            }
        }

        /// <summary>
        /// Reads all entries in Import table.
        /// </summary>
        private void ImportsOperation()
        {
            int loc = header.ImportPointer;
            Imports = new ImportEntry[header.ImportCount];
            for (int i = 0; i < Imports.Length; i++)
            {
                Imports[i].PackageIndex = ReadInt32(loc);
                Imports[i].Unknown1 = ReadInt32(loc + 4);
                Imports[i].ObjTypeIndex = ReadInt32(loc + 8);
                Imports[i].Unknown2 = ReadInt32(loc + 12);
                Imports[i].OwnerRef = ReadInt32(loc + 16);
                Imports[i].NameIndex = ReadInt32(loc + 20);
                Imports[i].Unknown3 = ReadInt32(loc + 24);
                loc += 28;
            }
        }

        public string GetTextImport(int idx)
        {
            string strImport = "Package name: " + Names[Imports[idx].PackageIndex] + Environment.NewLine;
            strImport += "Unknown1: " + Imports[idx].Unknown1.ToString("X8") + Environment.NewLine;
            strImport += "Class name: " + Names[Imports[idx].ObjTypeIndex] + Environment.NewLine;
            strImport += "Unknown2: " + Imports[idx].Unknown2.ToString("X8") + Environment.NewLine;
            strImport += "Link: " + Imports[idx].OwnerRef.ToString("X8") + Environment.NewLine;
            strImport += "Import name: " + Names[Imports[idx].NameIndex] + Environment.NewLine;
            strImport += "Unknown3: " + Imports[idx].Unknown3.ToString("X8");
            return strImport;
        }

        /// <summary>
        /// Reads all entries in Export table.
        /// </summary>
        private void ExportsOperation()
        {
            int loc = header.ExportPointer;
            Exports = new ExportEntry[header.ExportCount];
            for (int i = 0; i < Exports.Length; i++)
            {
                Exports[i].ObjTypeRef = ReadInt32(loc);
                Exports[i].ParentClassRef = ReadInt32(loc + 4);
                Exports[i].OwnerRef = ReadInt32(loc + 8);
                Exports[i].NameIndex = ReadInt32(loc + 0xC);
                Exports[i].NameCount = ReadInt32(loc + 0x10);
                Exports[i].Unk14 = ReadInt32(loc + 0x14);
                Exports[i].Unk18 = ReadInt32(loc + 0x18);
                Exports[i].Int1C = ReadInt32(loc + 0x1C);
                Exports[i].DataSize = ReadInt32(loc + 0x20);
                Exports[i].DataOffset = ReadInt32(loc + 0x24);
                Exports[i].Unk28 = ReadInt32(loc + 0x28);
                Exports[i].ExtraFieldsCount = ReadInt32(loc + 0x2C);
                Exports[i].GUID = ReadByteArray(loc + 0x30, 0x10);
                Exports[i].Flags = ReadInt32(loc + 0x40);
                if (Exports[i].ExtraFieldsCount > 0)
                {
                    Exports[i].Extra = new int[Exports[i].ExtraFieldsCount];
                    for (int j = 0; j < Exports[i].ExtraFieldsCount; j++)
                        Exports[i].Extra[j] = ReadInt32(loc + 0x44 + j * 4);
                }
                loc += 0x44 + Exports[i].ExtraFieldsCount * 4;
            }
        }

        public string GetTextExport(int idx)
        {
            //string strExport = "Package name: " + Names[Exports[idx].PackageNameID] + Environment.NewLine;
            string strExport = "Package name: " + Exports[idx].ObjTypeRef.ToString("X8") + Environment.NewLine;
            strExport += "Int04: " + Exports[idx].ParentClassRef.ToString("X8") + Environment.NewLine;
            strExport += "Link: " + Exports[idx].OwnerRef.ToString("X8") + Environment.NewLine;
            strExport += "Class name: " + Names[Exports[idx].NameIndex] + Environment.NewLine;
            strExport += "Int10: " + Exports[idx].NameCount.ToString("X8") + Environment.NewLine;
            strExport += "Unk14: " + Exports[idx].Unk14.ToString("X8") + Environment.NewLine;
            strExport += "Unk18: " + Exports[idx].Unk18.ToString("X8") + Environment.NewLine;
            strExport += "Int1C: " + Exports[idx].Int1C.ToString("X8") + Environment.NewLine;
            strExport += "Data size: " + Exports[idx].DataSize.ToString("X8") + Environment.NewLine;
            strExport += "Data offset: " + Exports[idx].DataOffset.ToString("X8") + Environment.NewLine;
            strExport += "Unknown3: " + Exports[idx].Unk28.ToString("X8") + Environment.NewLine;
            strExport += "Count: " + Exports[idx].ExtraFieldsCount.ToString("X8") + Environment.NewLine;
            strExport += "GUID: " + BitConverter.ToString(Exports[idx].GUID).ToUpper().Replace("-", String.Empty) + Environment.NewLine;
            strExport += "Flags: " + Exports[idx].Flags.ToString("X8");
            return strExport;
        }

        public byte[] GetNameTable()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bs = new BinaryWriter(ms);
            for (int i = 0; i < Names.Length; i++)
            {
                bs.Write((Names[i].Length + 1) * -1);
                byte[] strBytes = System.Text.Encoding.Unicode.GetBytes(Names[i]);
                bs.Write(strBytes);
                bs.Write((short)0);
            }
            return ms.ToArray();
        }

        private void ObjDataOperation()
        {
            ObjectDataFiles = new string[Exports.Length];
            for (int i = 0; i < Exports.Length; i++)
            {
                ObjectDataFiles[i] = String.Format("Data{0:D8}.bin", i);
            }
        }

        public bool ExportXML(string targetFile)
        {
            try
            {
                FolderPath = Path.GetDirectoryName(targetFile);
                System.IO.Directory.CreateDirectory(Path.Combine(FolderPath, ObjectFolder));
                XmlDocument xmldoc = new XmlDocument();
                // PCC
                XmlNode pccnode = xmldoc.CreateElement("PCC");
                // HEADER
                XmlNode headernode = xmldoc.CreateElement("Header");
                headernode.AppendChild(CreateNodeText(xmldoc, "UPK_SIGNATURE", HexString(header.MagicNumber)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Version", HexString(header.Version)));
                headernode.AppendChild(CreateNodeText(xmldoc, "PackageFlags", HexString(header.PackageFlags)));
                headernode.AppendChild(CreateNodeText(xmldoc, "NameCount", Names.Length.ToString() ));
                headernode.AppendChild(CreateNodeText(xmldoc, "ExportCount", Exports.Length.ToString() ));
                headernode.AppendChild(CreateNodeText(xmldoc, "ImportCount", Imports.Length.ToString() ));
                headernode.AppendChild(CreateNodeText(xmldoc, "GUID", BitConverter.ToString(header.GUID).ToUpper().Replace("-", String.Empty) ));
                headernode.AppendChild(CreateNodeText(xmldoc, "Generations", header.Generations.ToString()));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk62", HexString(header.Unk62)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk66", HexString(header.Unk66)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk6A", HexString(header.Unk6A)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk6E", HexString(header.Unk6E)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk72", HexString(header.Unk72)));
                headernode.AppendChild(xmldoc.CreateComment(GetVersionText()));
                headernode.AppendChild(CreateNodeText(xmldoc, "GameVersion", HexString(header.GameVersion)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk7A", HexString(header.Unk7A)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk7E", HexString(header.Unk7E)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk82", HexString(header.Unk82)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk86", HexString(header.Unk86)));
                headernode.AppendChild(CreateNodeText(xmldoc, "Unk8A", HexString(header.Unk8A)));
                pccnode.AppendChild(headernode);
                // NAMES
                XmlNode namenode = xmldoc.CreateElement("Names");
                for (int n = 0; n < Names.Length; n++)
                {
                    namenode.AppendChild(CreateNodeText(xmldoc, "Name" + n, Names[n]));
                }
                pccnode.AppendChild(namenode);
                // IMPORTS
                XmlNode importnode = xmldoc.CreateElement("Imports");
                for (int i = 0; i < Imports.Length; i++)
                {
                    XmlNode currentimport = xmldoc.CreateElement("Import" + i);
                    currentimport.AppendChild(xmldoc.CreateComment(GetFullNameImport(i)));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "PackageIndex", Imports[i].PackageIndex.ToString() ));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "Unknown1", Imports[i].Unknown1.ToString()));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "ObjTypeIndex", Imports[i].ObjTypeIndex.ToString()));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "Unknown2", Imports[i].Unknown2.ToString()));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "OwnerRef", Imports[i].OwnerRef.ToString()));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "NameIndex", Imports[i].NameIndex.ToString()));
                    currentimport.AppendChild(CreateNodeText(xmldoc, "Unknown3", Imports[i].Unknown3.ToString()));
                    importnode.AppendChild(currentimport);
                }
                pccnode.AppendChild(importnode);
                // EXPORTS
                XmlNode exportnode = xmldoc.CreateElement("Exports");
                for (int e = 0; e < Exports.Length; e++)
                {
                    XmlNode currentelement = xmldoc.CreateElement("Export" + e);
                    currentelement.AppendChild(xmldoc.CreateComment(GetFullNameExport(e)));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "Data", Path.Combine(ObjectFolder, ObjectDataFiles[e])));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "ObjTypeRef", Exports[e].ObjTypeRef.ToString() ));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "ParentClassRef", Exports[e].ParentClassRef.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "OwnerRef", Exports[e].OwnerRef.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "NameIndex", Exports[e].NameIndex.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "NameCount", Exports[e].NameCount.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "Unk14", Exports[e].Unk14.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "Unk18", Exports[e].Unk18.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "Int1C", Exports[e].Int1C.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "Unk28", Exports[e].Unk28.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "ExtraFieldsCount", Exports[e].ExtraFieldsCount.ToString()));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "GUID", BitConverter.ToString(Exports[e].GUID).ToUpper().Replace("-", String.Empty)));
                    currentelement.AppendChild(CreateNodeText(xmldoc, "Flags", HexString(Exports[e].Flags)));
                    for (int f = 0; f < Exports[e].ExtraFieldsCount; f++)
                    {
                        currentelement.AppendChild(CreateNodeText(xmldoc, "Extra" + f, Exports[e].Extra[f].ToString()));
                    }
                    exportnode.AppendChild(currentelement);
                    CreateExportDataFile(e);
                }
                pccnode.AppendChild(exportnode);

                xmldoc.AppendChild(pccnode);
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                XmlWriter writer = XmlWriter.Create(targetFile, settings);
                xmldoc.Save(writer);
                writer.Close();
                Form1.Log("XML created: " + Path.GetFileName(targetFile), Color.LightGreen);
                return true;
            }
            catch (Exception ex)
            {
                Form1.Log("ExportXML | " + ex.GetType().Name + ": " + ex.Message, Form1.errorColor);
                return false;
            }
        }

        private string GetVersionText()
        {
            ushort hiword = (ushort)(header.GameVersion >> 16);
            ushort loword = (ushort)(header.GameVersion & 0xFFFF);
            return String.Format(" GameVersion: 0x{0:X4} = {0:D}, 0x{1:X4} = {1:D} | {0:D}.{1:D} ", hiword, loword);
        }

        private XmlNode CreateNodeText(XmlDocument xmldoc, string name, string innerText)
        {
            XmlNode node = xmldoc.CreateElement(name);
            node.InnerText = innerText;
            return node;
        }

        private string HexString(int value)
        {
            return "0x" + value.ToString("X8");
        }

        private int HexInt32(string hexString)
        {
            return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        public string GetFullNameExport(int index)
        {
            string type = Exports[index].ObjTypeRef == 0 ? "Class": GetObjectName(Exports[index].ObjTypeRef);
            //string parent = GetObjectName(Exports[index].ParentClassRef);
            string owner = GetObjectName(Exports[index].OwnerRef);
            string name = Names[Exports[index].NameIndex];
            if (Exports[index].NameCount != 0)
                name += "_" + (Exports[index].NameCount - 1);
            if (Exports[index].OwnerRef != 0)
                return String.Format("{0} {1}.{2}", type, owner, name);
            else
                return String.Format("{0} {1}", type, name);
        }

        public string GetFullNameImport(int index)
        {
            string package = Names[Imports[index].PackageIndex];
            string class_ = Names[Imports[index].ObjTypeIndex];
            string owner = GetObjectName(Imports[index].OwnerRef);
            string name = Names[Imports[index].NameIndex];
            if (Imports[index].OwnerRef != 0)
                return String.Format("{0} | {1} {2}.{3}", package, class_, owner, name);
            else
                return String.Format("{0} | {1} {2}", package, class_, name);
        }

        public string GetObjectName(int objRef)
        {
            if (objRef == 0)
                return "(null)";
            if (objRef < 0)
            {
                objRef *= -1;
                objRef--;
                return Names[Imports[objRef].NameIndex];
            }
            objRef--;
            return Names[Exports[objRef].NameIndex];
        }

        public void CreateExportDataFile(int index)
        {
            byte[] data = new byte[Exports[index].DataSize];
            Array.Copy(RawContents, Exports[index].DataOffset, data, 0, data.Length);
            File.WriteAllBytes(Path.Combine(FolderPath, ObjectFolder, ObjectDataFiles[index]), data);
        }

        public void LoadExportDataFile(int index)
        {
            byte[] data = File.ReadAllBytes(Path.Combine(FolderPath, ObjectDataFiles[index]));
            Exports[index].DataSize = data.Length;
            Exports[index].DataOffset = ObjectData.Count;
            ObjectData.AddRange(data);
        }

        public void AdjustExportDataOffset(int offset)
        {
            for (int i = 0; i < Exports.Length; i++)
                Exports[i].DataOffset += offset;
        }


        public static void ConvertXMLToPCC(string xmlFile, string pccFile)
        {
            try
            {
                Form1.Log("Reading XML: " + Path.GetFileName(xmlFile));
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(xmlFile);
                PCC pcc = new PCC();
                pcc.FolderPath = Path.GetDirectoryName(xmlFile);
                // HEADER
                pcc.header.MagicNumber = XML_ReadInt32(xmldoc, "//PCC/Header/UPK_SIGNATURE");
                pcc.header.Version = XML_ReadInt32(xmldoc, "//PCC/Header/Version");
                pcc.header.PackageFlags = XML_ReadInt32(xmldoc, "//PCC/Header/PackageFlags");
                pcc.header.NameCount = XML_ReadInt32(xmldoc, "//PCC/Header/NameCount");
                pcc.header.ExportCount = XML_ReadInt32(xmldoc, "//PCC/Header/ExportCount");
                pcc.header.ImportCount = XML_ReadInt32(xmldoc, "//PCC/Header/ImportCount");
                pcc.header.GUID = XML_ReadGUID(xmldoc, "//PCC/Header/GUID");
                pcc.header.Generations = XML_ReadInt32(xmldoc, "//PCC/Header/Generations");
                pcc.header.Unk62 = XML_ReadInt32(xmldoc, "//PCC/Header/Unk62");
                pcc.header.Unk66 = XML_ReadInt32(xmldoc, "//PCC/Header/Unk66");
                pcc.header.Unk6A = XML_ReadInt32(xmldoc, "//PCC/Header/Unk6A");
                pcc.header.Unk6E = XML_ReadInt32(xmldoc, "//PCC/Header/Unk6E");
                pcc.header.Unk72 = XML_ReadInt32(xmldoc, "//PCC/Header/Unk72");
                pcc.header.GameVersion = XML_ReadInt32(xmldoc, "//PCC/Header/GameVersion");
                pcc.header.Unk7A = XML_ReadInt32(xmldoc, "//PCC/Header/Unk7A");
                pcc.header.Unk7E = XML_ReadInt32(xmldoc, "//PCC/Header/Unk7E");
                pcc.header.Unk82 = XML_ReadInt32(xmldoc, "//PCC/Header/Unk82");
                pcc.header.Unk86 = XML_ReadInt32(xmldoc, "//PCC/Header/Unk86");
                pcc.header.Unk8A = XML_ReadInt32(xmldoc, "//PCC/Header/Unk8A");
                // NAMES
                pcc.Names = new string[pcc.header.NameCount];
                for (int n = 0; n < pcc.Names.Length; n++)
                {
                    pcc.Names[n] = XML_ReadString(xmldoc, "//PCC/Names/Name" + n.ToString());
                }
                // IMPORTS
                pcc.Imports = new ImportEntry[pcc.header.ImportCount];
                for (int i = 0; i < pcc.Imports.Length; i++)
                {
                    pcc.Imports[i].PackageIndex = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/PackageIndex");
                    pcc.Imports[i].Unknown1 = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/Unknown1");
                    pcc.Imports[i].ObjTypeIndex = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/ObjTypeIndex");
                    pcc.Imports[i].Unknown2 = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/Unknown2");
                    pcc.Imports[i].OwnerRef = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/OwnerRef");
                    pcc.Imports[i].NameIndex = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/NameIndex");
                    pcc.Imports[i].Unknown3 = XML_ReadInt32(xmldoc, "//PCC/Imports/Import" + i + "/Unknown3");
                }
                // EXPORTS
                pcc.Exports = new ExportEntry[pcc.header.ExportCount];
                pcc.ObjectData.Clear();
                pcc.ObjectDataFiles = new string[pcc.header.ExportCount];
                for (int e = 0; e < pcc.Exports.Length; e++)
                {
                    pcc.Exports[e].ObjTypeRef = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/ObjTypeRef");
                    pcc.Exports[e].ParentClassRef = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/ParentClassRef");
                    pcc.Exports[e].OwnerRef = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/OwnerRef");
                    pcc.Exports[e].NameIndex = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/NameIndex");
                    pcc.Exports[e].NameCount = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/NameCount");
                    pcc.Exports[e].Unk14 = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/Unk14");
                    pcc.Exports[e].Unk18 = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/Unk18");
                    pcc.Exports[e].Int1C = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/Int1C");
                    pcc.Exports[e].Unk28 = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/Unk28");
                    pcc.Exports[e].ExtraFieldsCount = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/ExtraFieldsCount");
                    if (pcc.Exports[e].ExtraFieldsCount > 0)
                    {
                        pcc.Exports[e].Extra = new int[pcc.Exports[e].ExtraFieldsCount];
                        for (int f = 0; f < pcc.Exports[e].Extra.Length; f++)
                        {
                            pcc.Exports[e].Extra[f] = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/Extra" + f);
                        }
                    }
                    pcc.Exports[e].GUID = XML_ReadGUID(xmldoc, "//PCC/Exports/Export" + e + "/GUID");
                    pcc.Exports[e].Flags = XML_ReadInt32(xmldoc, "//PCC/Exports/Export" + e + "/Flags");
                    pcc.ObjectDataFiles[e] = XML_ReadString(xmldoc, "//PCC/Exports/Export" + e + "/Data");
                    pcc.LoadExportDataFile(e);
                }
                pcc.CalculateHeader();
                Form1.Log(String.Format("Gap: {0:D} (0x{0:X})", pcc.Gap), Color.LightBlue);
                pcc.AdjustExportDataOffset(pcc.header.HeaderSize + pcc.Gap);
                MemoryStream pccstream = new MemoryStream();
                BinaryWriter pccwriter = new BinaryWriter(pccstream);
                pccwriter.Write(pcc.GetHeader());
                pccwriter.Write(pcc.GetNameTable());
                pccwriter.Write(pcc.GetImportTable());
                pccwriter.Write(pcc.GetExportTable());
                pccwriter.Write(pcc.ObjectData.ToArray());
                File.WriteAllBytes(pccFile, pccstream.ToArray());
                Form1.Log("PCC created: " + Path.GetFileName(pccFile), Color.LightGreen);
            }
            catch (Exception ex)
            {
                Form1.Log("ConvertXMLToPCC | " + ex.GetType().Name + ": " + ex.Message, Form1.errorColor);
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        private static int XML_ReadInt32(XmlDocument xmldoc, string xpath)
        {
            XmlNode node = xmldoc.SelectSingleNode(xpath);
            if (node.InnerText.StartsWith("0x"))
            {
                return Convert.ToInt32(node.InnerText, 16);
            }
            return int.Parse(node.InnerText);
        }

        private static string XML_ReadString(XmlDocument xmldoc, string xpath)
        {
            XmlNode node = xmldoc.SelectSingleNode(xpath);
            return node.InnerText;
        }

        private static byte[] XML_ReadGUID(XmlDocument xmldoc, string xpath)
        {
            byte[] res = new byte[16];
            XmlNode node = xmldoc.SelectSingleNode(xpath);
            for (int i = 0; i < 16; i++)
            {
                string xs = node.InnerText.Substring(i * 2, 2);
                res[i] = byte.Parse(xs, System.Globalization.NumberStyles.HexNumber);
            }
            return res;
        }

        private void CalculateHeader()
        {
            int nametablesize = GetNameTable().Length;
            int importtablesize = Imports.Length * 28;
            int exporttablesize = Exports.Length * 0x44;
            Form1.Log(String.Format("Name table size: {0:D} (0x{0:X})", nametablesize), Color.LightBlue);
            Form1.Log(String.Format("Import table size: {0:D} (0x{0:X})", importtablesize), Color.LightBlue);
            Form1.Log(String.Format("Export table size: {0:D} (0x{0:X})", exporttablesize), Color.LightBlue);
            foreach (ExportEntry ee in Exports)
                exporttablesize += ee.ExtraFieldsCount * 4;
            //-----------------------------------------------
            header.HeaderSize = 0x8E + nametablesize + importtablesize + exporttablesize;
            header.HeaderSize1 = header.HeaderSize;
            header.HeaderSize2 = header.HeaderSize;
            Form1.Log(String.Format("Calculated header size: {0:D} (0x{0:X})", header.HeaderSize), Color.LightBlue);
            //-----------------------------------------------
            header.NamePointer = 0x8E;
            header.ImportPointer = header.NamePointer + nametablesize;
            header.ExportPointer = header.ImportPointer + importtablesize;

        }

        public byte[] GetHeader()
        {
            MemoryStream memstream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memstream, UnicodeEncoding.Unicode);
            bw.Write(header.MagicNumber);
            bw.Write(header.Version);
            bw.Write(header.HeaderSize); // size of (Header + Names + Imports + Exports)
            bw.Write(-5); // folder name size, including null char terminator (negative = unicode)
            bw.Write("None".ToCharArray()); // folder name, None = null
            bw.Write((short)0);
            bw.Write(header.PackageFlags);
            bw.Write(0);
            bw.Write(header.NameCount);
            bw.Write(header.NamePointer);
            bw.Write(header.ExportCount);
            bw.Write(header.ExportPointer);
            bw.Write(header.ImportCount);
            bw.Write(header.ImportPointer);
            bw.Write(header.HeaderSize1);
            bw.Write(header.HeaderSize2);
            bw.Write(0);
            bw.Write(0);
            bw.Write(0);
            bw.Write(header.GUID);
            bw.Write(header.Generations);
            bw.Write(header.Unk62);
            bw.Write(header.Unk66);
            bw.Write(header.Unk6A);
            bw.Write(header.Unk6E);
            bw.Write(header.Unk72);
            bw.Write(header.GameVersion);
            bw.Write(header.Unk7A);
            bw.Write(header.Unk7E);
            bw.Write(header.Unk82);
            bw.Write(header.Unk86);
            bw.Write(header.Unk8A);
            return memstream.ToArray(); 
        }

        public byte[] GetImportTable()
        {
            MemoryStream memstream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memstream);
            for (int i = 0; i < Imports.Length; i++)
            {
                bw.Write(Imports[i].PackageIndex);
                bw.Write(Imports[i].Unknown1);
                bw.Write(Imports[i].ObjTypeIndex);
                bw.Write(Imports[i].Unknown2);
                bw.Write(Imports[i].OwnerRef);
                bw.Write(Imports[i].NameIndex);
                bw.Write(Imports[i].Unknown3);
            }
            return memstream.ToArray();
        }

        public byte[] GetExportTable()
        {
            MemoryStream memstream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memstream);
            for (int i = 0; i < Exports.Length; i++)
            {
                bw.Write(Exports[i].ObjTypeRef);
                bw.Write(Exports[i].ParentClassRef);
                bw.Write(Exports[i].OwnerRef);
                bw.Write(Exports[i].NameIndex);
                bw.Write(Exports[i].NameCount);
                bw.Write(Exports[i].Unk14);
                bw.Write(Exports[i].Unk18);
                bw.Write(Exports[i].Int1C);
                bw.Write(Exports[i].DataSize);
                bw.Write(Exports[i].DataOffset);
                bw.Write(Exports[i].Unk28);
                bw.Write(Exports[i].ExtraFieldsCount);
                bw.Write(Exports[i].GUID);
                bw.Write(Exports[i].Flags);
                for (int j = 0; j < Exports[i].ExtraFieldsCount; j++)
                    bw.Write(Exports[i].Extra[j]);
            }
            bw.Write(new byte[Gap]);
            return memstream.ToArray();
        }

    }
}
