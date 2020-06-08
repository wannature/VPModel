import os
import sys
import config as cfg
import random
import json
def main():
    data_lebel=[]
    sum=0
    source_data_dir = cfg.CONFIG['source_data_dir']
    gesture_types = cfg.CONFIG['gesture_types'].split(',')
    for i in range(len(gesture_types)):
        data_dir = os.path.join(source_data_dir, str(gesture_types[i]))
        for files in os.walk(data_dir):
            for file in files[2]:
                data_lebel.append({"id":str(0), "image":file, "label":str(gesture_types[i])})
                sum+=1
    print (data_lebel)
    random.shuffle(data_lebel)
    id=0
    for i in range (sum):
        data_lebel[i]['id']=id
        id+=1
    print(data_lebel)
    data_label_dir = cfg.CONFIG['data_label_dir']
    if not os.path.exists(data_label_dir):
        os.system(r"touch {}".format(data_label_dir))
    with open(data_label_dir, 'w') as save_f:
        json.dump(data_lebel, save_f, ensure_ascii=False)

    with open(data_label_dir, 'r') as load_f:
        load_dict = json.load(load_f)

if __name__ == '__main__':
    main()