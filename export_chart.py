import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import datetime as dt
import sys


def export_chart(input_file_name, output_file_name):
    dates = []
    values = []

    with open(input_file_name, 'r') as file:
        for line in file:
            date, value = line.strip().split(',')
            dates.append(dt.datetime.strptime(date, "%Y-%m-%d").date())
            values.append(float(value))

    plt.figure(figsize=(8, 4.5))
    plt.plot(dates, values)

    axes = plt.gca()
    axes.xaxis.set_major_formatter(mdates.DateFormatter('%Y-%m-%d'))
    axes.xaxis.set_major_locator(mdates.AutoDateLocator())
    axes.set_title('USD/BYN')

    for label in axes.get_xticklabels(which='major'):
        label.set(rotation=30, horizontalalignment='right')

    plt.grid()
    plt.tight_layout()
    plt.savefig(output_file_name)


if __name__ == "__main__":
    export_chart(sys.argv[1], sys.argv[2])