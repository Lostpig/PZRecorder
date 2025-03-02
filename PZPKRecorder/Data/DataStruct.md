## version 0
+ t_kind
  + id: int
  + name: string

+ t_record
  + id: int
  + name: string
  + alias: string
  + kind: int // t_kind id
  + episode: int
  + episode_count: int
  + state: enum RecordState
  + remark: string
  + publish_year: int
  + publish_month: int
  + modify_date: DateTime

+ t_daily
  + id: int
  + name: string
  + alias: string
  + remark: string
  + state: enum DailyState
  + modify_date: DateTime

+ t_dailyweek
  + id: int // daily_id * 10000 + monday_day.from(2000,1,1).days()
  + daily_id: int // t_daily id  + monday_day: int  // DateOnly days
  + d1: int
  + d2: int
  + d3: int
  + d4: int
  + d5: int
  + d6: int
  + d7: int


## version 10001
+ t_kind
  > add order_no column
  + id: int
  + name: string
  + order_no: int

+ t_daily
  > add order_no column
  + id: int
  + name: string
  + alias: string
  + remark: string
  + state: enum DailyState
  + modify_date: DateTime
  + order_no: int

## version 10002
+ t_dailyweek
  > id generate change to daily_id * 100000 + monday_day.from(2000,1,1).days()

## version 10003
+ t_kind
  > add custom state name columns
  + id: int
  + name: string
  + order_no: int
  + state_wish: string
  + state_doing: string
  + state_complete: string
  + state_giveup: string

## version 10004
+ t_daily
  > modify remark length to 1000  
  > add start_day and end_day columns  
  + id: int
  + name: string
  + alias: string
  + remark: string
  + start_day: int
  + end_day: int
  + state: enum DailyState
  + modify_date: DateTime
  + order_no: int