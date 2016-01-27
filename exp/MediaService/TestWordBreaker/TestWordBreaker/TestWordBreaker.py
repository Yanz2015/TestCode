import jieba
import dataaccess

def word_breaker_test():
    fname = "D:/Git/TestCode/Test/TestApplication/TestApplication/bin/Debug/0.csv"
    textname = "textfull.txt"
    segname = "seglistfull.txt"

    with open(fname, encoding='utf-8') as f:
        contents = f.readlines()

    textlist = []
    splitList = []

    print(len(contents))
    index = 1

    for item in contents:
        ary = str.split(item, ',')
        text = ary[18]
        textlist.append(text)
        seg_list = jieba.cut(text, cut_all=False)
        splitList.append("/ ".join(seg_list))
        print(index)
        index+=1

    with open(textname, "a", encoding='utf-8') as wt:
        wt.write('\n'.join(textlist))


    with open(segname, "a", encoding='utf-8') as segw:
        segw.write('\n'.join(splitList))


def filterTest():
    fname = "D:/Git/TestCode/Test/TestApplication/TestApplication/bin/Debug/data.txt"
    result = "breaklistfull.txt"
    with open(fname, encoding = 'utf-8') as f:
        contents = f.readlines()

    splitList = []

    print(len(contents))
    index = 1
    fileIndex = 1

    for item in contents:
        ary = str.split(item, ',')
        text = ary[1]
        seg_list = jieba.cut(text, cut_all=False)
        break_result = "/ ".join(seg_list)
        splitList.append(ary[0] + "," + break_result + "," + ary[2])
        if index % 1000 == 0:
            print(index)
        if index % 10000 == 0:
            with open("breaklist{0}.txt".format(fileIndex), "a", encoding='utf-8') as wt:
                wt.write('\n'.join(splitList))
            splitList = []
            fileIndex +=1
        index+=1

    with open(result, "a", encoding='utf-8') as wt:
        wt.write('\n'.join(splitList))


filterTest()