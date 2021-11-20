
## source by https://github.com/dsouzawilbur/Scripts/blob/master/Change_Photo_Taken_Time.py
import time
from datetime import datetime
import os, re, sys
import shutil
import piexif

def fix(directory):
    # *&* directory인지 확인하고 존재하는지 확인하여 아니면 에러 발생 처리 필요
    fw = open(os.path.join(directory, "Logs.txt"), 'w')

    print("RootDir: ", directory)
    fw.write("RootDir: " + directory + "\n")
    for dirpath, _, filenames in os.walk(directory):
        print("WalkDir: ", dirpath)
        fw.write("WalkDir: " + dirpath + "\n")
        for f in filenames:
            fullPath = os.path.abspath(os.path.join(dirpath, f))
            # pass json files
            if re.match(r".*\.json$", f) or re.match(r".*\.html$", f):
                continue

            # Format: 20170204200801.png
            elif re.match(r"^20\d\d\d\d\d\d\d\d\d\d\d\d.*", f):
                match = re.search("^(20\d\d)(\d\d)(\d\d)(\d\d)(\d\d)(\d\d).*", f)
                date = datetime(year=int(match.group(1)), month=int(match.group(2)),\
                    day=int(match.group(3)), hour=int(match.group(4)),\
                    minute=int(match.group(5)), second=int(match.group(6)))
                InternalUpdateFileProperties(fullPath, date)

            # Format: Screenshot_20170204-200801.png
            elif re.match(r"^Screenshot_\d\d\d\d\d\d\d\d-\d\d\d\d\d\d.*", f):
                match = re.search("^Screenshot_(\d\d\d\d)(\d\d)(\d\d)-(\d\d)(\d\d)(\d\d).*", f)
                date = datetime(year=int(match.group(1)), month=int(match.group(2)),\
                    day=int(match.group(3)), hour=int(match.group(4)),\
                    minute=int(match.group(5)), second=int(match.group(6)))
                InternalUpdateFileProperties(fullPath, date)

            # Format: IMG_20160117_091135.jpg
            elif re.match(r"^IMG_\d\d\d\d\d\d\d\d_\d\d\d\d\d\d.*", f):
                match = re.search("^IMG_(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d).*", f)
                date = datetime(year=int(match.group(1)), month=int(match.group(2)),\
                    day=int(match.group(3)), hour=int(match.group(4)),\
                    minute=int(match.group(5)), second=int(match.group(6)))
                InternalUpdateFileProperties(fullPath, date)

            # Format: 20170204_200801.png
            elif re.match(r"^\d\d\d\d\d\d\d\d_\d\d\d\d\d\d.*", f):
                match = re.search("^(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d).*", f)
                date = datetime(year=int(match.group(1)), month=int(match.group(2)),\
                    day=int(match.group(3)), hour=int(match.group(4)),\
                    minute=int(match.group(5)), second=int(match.group(6)))
                InternalUpdateFileProperties(fullPath, date)
            
            # Format: 2015-04-12_03.23.09.jpg
            elif re.match(r"^\d\d\d\d-\d\d-\d\d_\d\d\.\d\d\.\d\d.*", f):
                match = re.search("^(\d\d\d\d)-(\d\d)-(\d\d)_(\d\d)\.(\d\d)\.(\d\d).*", f)
                date = datetime(year=int(match.group(1)), month=int(match.group(2)),\
                    day=int(match.group(3)), hour=int(match.group(4)),\
                    minute=int(match.group(5)), second=int(match.group(6)))
                InternalUpdateFileProperties(fullPath, date)

            # Format: P140131_110610.jpg
            elif re.match(r"^P\d\d\d\d\d\d_\d\d\d\d\d\d.*", f):
                match = re.search("^P(\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d).*", f)
                date = datetime(year=int("20" + match.group(1)), month=int(match.group(2)),\
                    day=int(match.group(3)), hour=int(match.group(4)),\
                    minute=int(match.group(5)), second=int(match.group(6)))
                InternalUpdateFileProperties(fullPath, date)

            # Format: IMG-20160117-WA0001.jpg
            elif re.match(r"^IMG-\d\d\d\d\d\d\d\d-WA\d\d\d\d.*", f):
                #print(f+" Matched")
                match = re.search("^IMG-(\d\d\d\d)(\d\d)(\d\d)-WA\d\d\d\d.*", f)
                year = match.group(1)
                month= match.group(2)
                day = match.group(3)
                InternalUpdateFileProperties(fullPath, datetime(int(year), int(month), int(day), 4, 0, 0))

            else:
                # Format: DateTimeTaken Exist
                try:
                    if piexif.ExifIFD.DateTimeOriginal in piexif.load(fullPath)['Exif']:
                        date_time_str = piexif.load(fullPath)['Exif'][piexif.ExifIFD.DateTimeOriginal].decode("utf-8") 
                        date = datetime.strptime(date_time_str, "%Y:%m:%d %H:%M:%S")
                        InternalUpdateFileProperties(fullPath, date)
                    else:
                        raise Exception()
                # Unmatched File Log
                except:
                    print("Unmatched: ", f)
                    fw.write("Unmatched: " + f + "\n")
    print("Finished!!")
    fw.write("Finished!!\n")
    fw.close()

def InternalUpdateFileProperties(fullpath, date):
    if not isinstance(fullpath, str):
        raise TypeError("must be str, not %s" % type(fullpath).__name__)
    if not isinstance(date, datetime):
        raise TypeError("must be datetime, not %s" % type(date).__name__)

    try:
        exif_dict = piexif.load(fullpath)
        #Update DateTimeOriginal
        exif_dict['Exif'][piexif.ExifIFD.DateTimeOriginal] = date.strftime("%Y:%m:%d %H:%M:%S")
        #Update DateTimeDigitized
        exif_dict['Exif'][piexif.ExifIFD.DateTimeDigitized] = date.strftime("%Y:%m:%d %H:%M:%S")
        #Update DateTime
        exif_dict['0th'][piexif.ImageIFD.DateTime] = date.strftime("%Y:%m:%d %H:%M:%S")
        exif_bytes = piexif.dump(exif_dict)
        piexif.insert(exif_bytes, fullpath)
    except:
        filename = os.path.basename(fullpath)
        #print("ExifPass: " + filename)

    modTime = time.mktime(date.timetuple())
    #Update DateTimeModified, DateTimeCreated
    os.utime(fullpath, (modTime, modTime))

def InternalRenameFile(src, dst, filename):
    os.rename()

def InternalMoveFile(srcpath, dstpath):
    if not os.path.isfile(srcpath) or\
    not os.path.isfile(dstpath):
        raise TypeError("must be file path")
    #src = os.path.join(src, filename)
    shutil.move(srcpath, dstpath)

def InternalCopyFile(srcpath, dstpath):
    if not os.path.isfile(srcpath) or\
    not os.path.isfile(dstpath):
        raise TypeError("must be file path")
    # copy with meta data
    shutil.copy2(srcpath, dstpath)

def dlt(directory):
    print("dlt: ", directory)
    for dirpath, _, filenames in os.walk(directory):
        for f in filenames:
            fullPath = os.path.abspath(os.path.join(dirpath, f))
            # Format: Screenshot_20170204-200801.png
            if re.match(r".*\.json$", f):
                print(f)
                #os.remove(f)

if __name__ == "__main__":
    #fix(sys.argv[1])
    #dlt(r"C:\Users\zacal\Downloads\takeout-20210212T182356Z-001")
    fix(r"D:\photo\Google 포토")
    #"C:\Users\zacal\Downloads\20170401_120226.jpg"