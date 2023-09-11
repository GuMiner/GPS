import dataclasses
import queue
import z3
from pathlib import Path
from typing import Union
from pyvis.network import Network

def _read_file_lines(day) -> list[str]:
    return Path(f'data/{day}.txt').read_text().splitlines()


def day_1():
    # Part 1, increases in entries in a data set
    entries = _read_file_lines(1)
    depth_increased = 0

    previous_entry: Union[int, None] = None
    for entry in entries:
        if previous_entry is not None and int(entry) > int(previous_entry):
            depth_increased = depth_increased + 1
        previous_entry = int(entry)

    print(depth_increased)

    # Part 2, increases between 3-entry sliding windows
    depth_increased = 0
    old_window = queue.Queue()
    new_window = queue.Queue()
    start = True
    for entry in entries:
        # Delay so windows are appropriately sliding
        if start:
            start = False
        else:
            new_window.put(int(entry))

        if new_window.qsize() > 3:
            new_window.get() # drop oldest

        if new_window.qsize() == 3:
            # old_window guaranteed to have 3 elements at this point in time
            if sum(new_window.queue) > sum(old_window.queue):
                depth_increased = depth_increased + 1

        old_window.put(int(entry))
        if old_window.qsize() > 3:
            old_window.get()
    print(depth_increased)


def day_2():
    entries = _read_file_lines(2)
    horizontal_position = 0
    depth = 0

    def forward(amount):
        nonlocal horizontal_position
        horizontal_position = horizontal_position + amount

    def down(amount):
        nonlocal depth
        depth = depth + amount

    def up(amount):
        nonlocal depth
        depth = depth - amount

    def forward_aim(amount):
        nonlocal aim, depth, horizontal_position
        forward(amount)
        depth = depth + aim * amount

    def down_aim(amount):
        nonlocal aim
        aim = aim + amount

    def up_aim(amount):
        nonlocal aim
        aim = aim - amount

    entry: str
    for entry in entries:
        parts = entry.split(' ')

        {
            "forward": forward,
            "down": down,
            "up": up
        }[parts[0]](int(parts[1]))

    print(f'horiz: {horizontal_position}, depth: {depth}, x: {horizontal_position * depth}')

    # Part 2
    horizontal_position = 0
    depth = 0
    aim = 0  # For part 2
    for entry in entries:
        parts = entry.split(' ')

        {
            "forward": forward_aim,
            "down": down_aim,
            "up": up_aim
        }[parts[0]](int(parts[1]))

    print(f'horiz: {horizontal_position}, depth: {depth}, x: {horizontal_position * depth}')


def day_3():
    entries = _read_file_lines(3)
    bit_length = len(entries[0])

    significant_bits = []
    for i in range(bit_length):
        significant_bits.append(0)

    for entry in entries:
        for i in range(bit_length):
            significant_bits[i] = significant_bits[i] + \
                (-1 if entry[i] == '0' else 1)

    print(significant_bits)
    gr = ''.join(['0' if entry < 0 else '1' for entry in significant_bits])
    er = ''.join(['1' if entry < 0 else '0' for entry in significant_bits])

    gamma_rate = int(f'0b{gr}', 2)
    epsilon_rate = int(f'0b{er}', 2)
    print(gamma_rate * epsilon_rate)

    # Part 2
    oxy_entries = entries.copy()
    scrub_entries = entries.copy()
    for i in range(bit_length):
        significant_bit = 0
        for entry in oxy_entries:
            significant_bit = significant_bit + \
                (-1 if entry[i] == '0' else 1)
        bit = '0' if significant_bit < 0 else '1'
        oxy_entries = [entry for entry in oxy_entries if entry[i] == bit]
        if len(oxy_entries) == 1:
            break

    for i in range(bit_length):
        significant_bit = 0
        for entry in scrub_entries:
            significant_bit = significant_bit + \
                (-1 if entry[i] == '0' else 1)
        bit = '1' if significant_bit < 0 else '0'
        scrub_entries = [entry for entry in scrub_entries if entry[i] == bit]
        if len(scrub_entries) == 1:
            break

    oxy_rating = int(f'0b{oxy_entries[0]}', 2)
    scrub_rating = int(f'0b{scrub_entries[0]}', 2)
    print(oxy_rating * scrub_rating)

@dataclasses.dataclass
class Cell:
    value: int
    chosen: bool

def d4_parse_line(line: str) -> [Cell]:
    line_values = line.split()
    return [Cell(int(value), False) for value in line_values]

def d4_cross_out_number(board, number: int) -> bool:
    for i in range(5):
        for j in range(5):
            if board[i][j].value == number:
                    board[i][j].chosen = True

    for i in range(5):
        column_win = True
        for j in range(5):
            if not board[i][j].chosen:
                column_win = False
                break

        if column_win:
            return True

    for j in range(5):
        row_win = True
        for i in range(5):
            if not board[i][j].chosen:
                row_win = False
                break

        if row_win:
            return True


def d4_get_board_score(board, number: int) -> int:
    unmarked_sum = 0

    line: [Cell]
    for line in board:
        unmarked_sum = unmarked_sum + sum([
            cell.value for cell in line
            if not cell.chosen
        ])

    return unmarked_sum * number

def day_4():
    lines = _read_file_lines(4)
    numbers = [int(value) for value in lines[0].split(',')]
    boards = []

    board_lines = []
    for line in lines[1:]:
        if line.strip():
            board_lines.append(d4_parse_line(line))

        if len(board_lines) == 5:
            boards.append(board_lines.copy())
            board_lines = []

    for number in numbers:
        # print(number)
        won = False
        for board in boards:
            if d4_cross_out_number(board, number):
                print(d4_get_board_score(board, number))
                won = True

        if won:
            break

    # Part 2
    for board in boards:
        for i in range(5):
            for j in range(5):
                board[i][j].chosen = False


    won_boards = []
    for number in numbers:
        won_last = False
        for i in range(len(boards)):
            board = boards[i]
            if not i in won_boards:
                if d4_cross_out_number(board, number):
                    won_boards.append(i)
                    if len(won_boards) == len(boards):
                        print(d4_get_board_score(board, number))
                        # 11070 too low
                        won_last = True

        if won_last:
            break

def d5_add_point(pts: dict, x, y):
    if not x in pts:
        pts[x] = {}

    if not y in pts[x]:
        pts[x][y] = 0

    pts[x][y] = pts[x][y] + 1

def day_5():
    lines = _read_file_lines(5)
    points = {}

    for line in lines:
        line_start = [int(value) for value in line.split(' -> ')[0].split(',')]
        line_end = [int(value) for value in line.split(' -> ')[1].split(',')]
        if line_start[0] == line_end[0]:
            # Iterate on Y
            start_y = min(line_start[1], line_end[1])
            end_y = max(line_start[1], line_end[1])

            y = start_y
            while y <= end_y:
                d5_add_point(points, line_start[0], y)
                y = y + 1
        elif line_start[1] == line_end[1]:
            # Iterate on X
            start_x = min(line_start[0], line_end[0])
            end_x = max(line_start[0], line_end[0])

            x = start_x
            while x <= end_x:
                d5_add_point(points, x, line_start[1])
                x = x + 1
        else:
            # Diagonal line.
            # pass # Only for Part 2. Part 1 result: 5084

            # Orient the line so that start_x is always < end_x
            start_x = line_start[0]
            start_y = line_start[1]
            end_x = line_end[0]
            end_y = line_end[1]
            if start_x > end_x:
                start_x = line_end[0]
                start_y = line_end[1]
                end_x = line_start[0]
                end_y = line_start[1]

            # Increment X.
            inc_y = 1 if end_y >= start_y else -1
            x = start_x
            y = start_y
            while x <= end_x:
                d5_add_point(points, x, y) # 14296 is too low

                x = x + 1
                y = y + inc_y

    overlap_count = 0
    for x in points:
        overlap_count = overlap_count + sum(
            1 for y in points[x]
            if points[x][y] > 1
        )

    print(overlap_count)

    # 5063 is too low
    for x in points:
        for y in points[x]:
            if points[x][y] > 1:
                overlap_count = overlap_count - 1
    print(overlap_count)

@dataclasses.dataclass
class Fish:
    timer: int

def day_6():
    fishes = [Fish(int(value)) for value in _read_file_lines(6)[0].split(',')]

    current_day = 1
    while current_day <= 80:
        new_fish_count = 0
        for fish in fishes:
            fish.timer = fish.timer - 1
            if fish.timer < 0:
                fish.timer = 6
                new_fish_count = new_fish_count + 1
        while new_fish_count > 0:
            fishes.append(Fish(8))
            new_fish_count = new_fish_count - 1

        current_day = current_day + 1

    print(len(fishes))

    # Part 2
    # Need a smarter solution -- track in aggregate -- counting is too slow
    fishes = [Fish(int(value)) for value in _read_file_lines(6)[0].split(',')]

    fish_states = {}
    for fish in fishes:
        if not fish.timer in fish_states:
            fish_states[fish.timer] = 0

        fish_states[fish.timer] = fish_states[fish.timer] + 1

    current_day = 1
    while current_day <= 256: # Replace with 80 to verify
        new_fish_states = {}
        new_fish_states[8] = 0
        for timer_value in fish_states:
            new_timer_value = timer_value - 1
            if new_timer_value < 0:
                new_timer_value = 6
                new_fish_states[8] = new_fish_states[8] + fish_states[timer_value]

            if not new_timer_value in new_fish_states:
                new_fish_states[new_timer_value] = 0
            new_fish_states[new_timer_value] = new_fish_states[new_timer_value] + fish_states[timer_value]

        fish_states = new_fish_states.copy()
        current_day = current_day + 1

    print(fish_states)
    print(sum([fish_states[timer_value] for timer_value in fish_states]))

def day_7():
    crab_horiz = [int(value) for value in _read_file_lines(7)[0].split(',')]
    min_position = min(crab_horiz)
    max_position = max(crab_horiz)

    # Part 1
    pos = min_position
    min_fuel_used = None
    while pos <= max_position:
        # Compute used fuel for this position
        fuel_used = sum([abs(pos - crab_pos) for crab_pos in crab_horiz])
        if min_fuel_used is None or fuel_used < min_fuel_used:
            min_fuel_used = fuel_used
            # print(min_fuel_used)

        pos = pos + 1

    print(min_fuel_used)

    # Part 2
    pos = min_position
    min_fuel_used = None
    while pos <= max_position:
        # Compute used fuel for this position
        fuel_used = sum(
            # [abs(pos - crab_pos) for crab_pos in crab_horiz])
        [int((abs(pos - crab_pos) * (abs(pos - crab_pos) + 1))/2) for crab_pos in crab_horiz])
        if min_fuel_used is None or fuel_used < min_fuel_used:
            min_fuel_used = fuel_used
            # print(min_fuel_used)

        pos = pos + 1

    print(min_fuel_used)

def day_8():
    # 7-segment display
    #   0:      1:      2:      3:      4:
    #  aaaa    ....    aaaa    aaaa    ....
    # b    c  .    c  .    c  .    c  b    c
    # b    c  .    c  .    c  .    c  b    c
    #  ....    ....    dddd    dddd    dddd
    # e    f  .    f  e    .  .    f  .    f
    # e    f  .    f  e    .  .    f  .    f
    #  gggg    ....    gggg    gggg    ....    0, 6, 9. 6 == missing one of '1' in it
    #  6      2        5        5      4       # 0 = missing entry from '4' and not '6'
    #   5:      6:      7:      8:      9:     2, 3, 5, 3 == has all 7 entries
    #  aaaa    aaaa    aaaa    aaaa    aaaa   # 2 == has letter not in 6
    # b    .  b    .  .    c  b    c  b    c
    # b    .  b    .  .    c  b    c  b    c
    #  dddd    dddd    ....    dddd    dddd
    # .    f  e    f  .    f  e    f  .    f
    # .    f  e    f  .    f  e    f  .    f
    #  gggg    gggg    ....    gggg    gggg
    #   5       6        3      7       6

    # problem = Problem()
    # problem.addVariable("a", [1, 2, 3])
    # problem.addVariable("b", [4, 5, 6])
    # print(problem.getSolutions())
    lines = _read_file_lines(8)

    # Part 1
    count = 0
    for line in lines:
        parts = line.split(' | ')
        entries = parts[0].split()
        digits = parts[1].split()
        for digit in digits:
            if len(digit) in [2,4,3,7]: # 1, 4, 7, 8
                count = count + 1

    print(count)

    # Part 1:
    output_sum = 0
    for line in lines:
        parts = line.split(' | ')
        entries = parts[0].split()
        digits = parts[1].split()
        one_entry = [digit for digit in entries if len(digit) == 2][0]
        four_entry = [digit for digit in entries if len(digit) == 4][0]
        seven_entry = [digit for digit in entries if len(digit) == 3][0]
        eight_entry = [digit for digit in entries if len(digit) == 7][0]

        six_possibles = [digit for digit in entries if len(digit) == 6]
        six_entry: str = None
        for entry in six_possibles:
            for letter in one_entry:
                if letter not in entry:
                    six_entry = entry
                    break
            if six_entry:
                break

        zero_entry: str = None
        for entry in six_possibles:
            if entry == six_entry:
                continue

            for letter in four_entry:
                if letter not in entry:
                    zero_entry = entry
                    break

            if zero_entry:
                break

        nine_entry = [entry for entry in six_possibles if entry != zero_entry and entry != six_entry][0]

        five_possibles = [digit for digit in entries if len(digit) == 5]
        three_entry: str = None
        for entry in five_possibles:
            is_three = True
            for letter in seven_entry:
                if letter not in entry:
                    is_three = False
                    break

            if is_three:
                three_entry = entry
                break

        two_entry: str = None
        for entry in five_possibles:
            if entry is three_entry:
                continue

            for letter in entry:
                if letter not in six_entry:
                    two_entry = entry
                    break

            if two_entry:
                break

        five_entry = [entry for entry in five_possibles if entry != two_entry and entry != three_entry][0]

        # print(f'{one_entry} {four_entry} {seven_entry} {eight_entry} {six_entry} {zero_entry} {nine_entry} {three_entry} {two_entry} {five_entry}')

        digit_map = {
            ''.join(sorted(one_entry)): 1,
            ''.join(sorted(two_entry)): 2,
            ''.join(sorted(three_entry)): 3,
            ''.join(sorted(four_entry)): 4,
            ''.join(sorted(five_entry)): 5,
            ''.join(sorted(six_entry)): 6,
            ''.join(sorted(seven_entry)): 7,
            ''.join(sorted(eight_entry)): 8,
            ''.join(sorted(nine_entry)): 9,
            ''.join(sorted(zero_entry)): 0
        }
        # print(digit_map)

        value = 0
        multiplier = 1000
        for digit in digits:
            value = value + multiplier * digit_map[''.join(sorted(digit))]
            multiplier = multiplier / 10

        # print(int(value))
        output_sum = output_sum + value
        # top_letter = seven_entry.replace(one_entry[0], '').replace(one_entry[1], '')
    print(output_sum)

def d9_is_low(x: int, y: int, digits):
    value = digits[x][y]

    left_value = digits[x - 1][y] if x > 0 else None
    right_value = digits[x + 1][y] if x + 1 < len(digits) else None
    up_value = digits[x][y - 1] if y > 0 else None
    down_value = digits[x][y + 1] if y + 1 < len(digits) else None

    surrounding_values = [left_value, right_value, up_value, down_value]

    for surrounding_value in surrounding_values:
        if surrounding_value is not None and value >= surrounding_value:
            return False

    return True

@dataclasses.dataclass(frozen=True)
class Point:
    x: int
    y: int

def d9_try_add_point(x: int, y: int, digits, scanned_points: set[Point], points_to_scan):
    if x < 0 or y < 0 or x >= len(digits) or y >= len(digits):
        return # Out of bounds

    if digits[x][y] == 9:
        return # Not in any basin

    if Point(x, y) in scanned_points:
        return # Already scanned

    # Must be a valid thing to add
    points_to_scan.put(Point(x, y))

def d9_find_basin_size(x: int, y: int, digits):
    # Scan digits to find areas until hitting an edge or a 9.
    scanned_points = set()
    points_to_scan = queue.Queue()
    points_to_scan.put(Point(x, y))

    while points_to_scan.qsize() > 0:
        # BFS or DFS doesn't matter as we're enumerating the whole search space
        next_point: Point = points_to_scan.get()
        scanned_points.add(next_point)

        d9_try_add_point(next_point.x - 1, next_point.y, digits, scanned_points, points_to_scan)
        d9_try_add_point(next_point.x + 1, next_point.y, digits, scanned_points, points_to_scan)
        d9_try_add_point(next_point.x, next_point.y - 1, digits, scanned_points, points_to_scan)
        d9_try_add_point(next_point.x, next_point.y + 1, digits, scanned_points, points_to_scan)

    return len(scanned_points)

def day_9():
    lines = _read_file_lines(9)
    digits = []
    for line in lines:
        digits.append([int(c) for c in line])

    risk_sum = 0
    low_points = []
    for x in range(len(digits)):
        for y in range(len(digits)):
            if d9_is_low(x,y,digits):
                risk_sum = risk_sum + digits[x][y] + 1
                low_points.append(Point(x, y))

    print(risk_sum)

    # Part 2
    basin_sizes = []
    for point in low_points:
        basin_size = d9_find_basin_size(point.x, point.y, digits)
        basin_sizes.append(basin_size)

    basin_sizes = sorted(basin_sizes, reverse=True)
    print(basin_sizes[0] * basin_sizes[1] * basin_sizes[2])

def day_10():
    lines = _read_file_lines('10')

    incomplete_lines = []
    closing_tag_map = {
        '(': ')',
        '[': ']',
        '{': '}',
        '<': '>'
    }

    syntax_error_score = 0
    for line in lines:
        corrupted = False
        syntax_checker = queue.LifoQueue()
        for value in line:
            if value in ['(', '[', '{', '<']:
                syntax_checker.put(value)
            else:
                if syntax_checker.qsize() == 0:
                    # No elements to get and this is a closing character
                    # Not expected to happen
                    raise('Should not get here')

                expected_value = closing_tag_map[syntax_checker.get()]
                if value is not expected_value:
                    # Syntax violation
                    score_table = {
                        ')': 3,
                        ']': 57,
                        '}': 1197,
                        '>': 25137
                    }
                    syntax_error_score = syntax_error_score + score_table[value]
                    corrupted = True
                    break

        if not corrupted:
            incomplete_lines.append(line)

    print(syntax_error_score)

    # Part 2
    incomplete_scores = []
    for line in incomplete_lines:
        syntax_checker = queue.LifoQueue()
        for value in line:
            if value in ['(', '[', '{', '<']:
                syntax_checker.put(value)
            else:
                # Assume correct from Part 1, just incomplete
                syntax_checker.get()

        incomplete_score = 0
        while syntax_checker.qsize() > 0:
            incomplete_score = incomplete_score * 5
            closing_character = closing_tag_map[syntax_checker.get()]
            point_map = {
                ')': 1,
                ']': 2,
                '}': 3,
                '>': 4
            }
            incomplete_score = incomplete_score + point_map[closing_character]

        incomplete_scores.append(incomplete_score)

    # Print the middle score
    print(sorted(incomplete_scores)[int(len(incomplete_scores) / 2)])

def d11_increase_if_not_zero(x, y, grid):
    if x < 0 or y < 0 or x + 1 > len(grid) or y + 1 > len(grid):
        return # Out of bounds

    if grid[x][y] == 0:
        return # Already flashed, so ignore it

    grid[x][y] = grid[x][y] + 1

def d11_apply_flash(x, y, grid):
    d11_increase_if_not_zero(x - 1, y - 1, grid)
    d11_increase_if_not_zero(x, y - 1, grid)
    d11_increase_if_not_zero(x + 1, y - 1, grid)

    d11_increase_if_not_zero(x - 1, y, grid)
    d11_increase_if_not_zero(x + 1, y, grid)

    d11_increase_if_not_zero(x - 1, y + 1, grid)
    d11_increase_if_not_zero(x, y + 1, grid)
    d11_increase_if_not_zero(x + 1, y + 1, grid)

def d11_any_can_flash(grid):
    for x in range(len(grid)):
        for y in range(len(grid)):
            if grid[x][y] > 9:
                return True

    return False

def d11_step(grid):
    flashed_this_step = 0
    for x in range(len(grid)):
        for y in range(len(grid)):
            grid[x][y] = grid[x][y] + 1

    while d11_any_can_flash(grid):
        for x in range(len(grid)):
            for y in range(len(grid)):
                if grid[x][y] > 9:
                    grid[x][y] = 0
                    flashed_this_step = flashed_this_step + 1
                    d11_apply_flash(x, y, grid)

    return flashed_this_step

def d11_print_grid(grid):
    for line in grid:
        print(line)

    print()

def day_11():
    lines = _read_file_lines('11')
    grid = []
    for line in lines:
        grid.append([int(value) for value in line])

    flash_count = 0
    iterations = 0
    all_flash = False # Part 2
    while True:
        new_grid = []
        for line in grid:
            new_grid.append(line.copy())
        flashes = d11_step(new_grid)
        grid = new_grid.copy()

        flash_count = flash_count + flashes
        iterations = iterations + 1
        if iterations == 100: # Part 1
            print(flash_count)

        if flashes == len(grid) * len(grid): # Part 2
            print(iterations)
            break

def d12_is_route_valid(route):
    for node in route:
        if node.islower() and route.count(node) > 1:
            # Conveniently, this also returns 'False' if 'start' happens twice.
            return False

    return True

def d12_is_route_finished(route):
    if len(route) >= 2:
        if route[0] == 'start' and route[-1] == 'end':
            return True

    return False

@dataclasses.dataclass
class RouteWithSingleVisit:
    route: [str]
    has_visited_lower_twice: bool

def d12_is_route2_valid(route: RouteWithSingleVisit):
    nodes_visited_twice = {}
    for node in route.route:
        if node.islower() and route.route.count(node) > 1:
            if node == 'start' or node == 'end':
                # Both 'start' and 'end' cannt be visited twice.
                return False

            if node not in nodes_visited_twice:
                nodes_visited_twice[node] = 0

            nodes_visited_twice[node] = nodes_visited_twice[node] + 1

    # Invalid 'has_visited_lower_twice' setting
    if not any(nodes_visited_twice) and route.has_visited_lower_twice:
        return False
    if any(nodes_visited_twice) and not route.has_visited_lower_twice:
        return False

    # Only one lowercase node can be visited twice.
    if len(nodes_visited_twice) > 1:
        return False

    for key in nodes_visited_twice:
        if nodes_visited_twice[key] > 2:
            return False # Cannot visit a small node more than twice

    return True

def d12_is_route2_finished(route: RouteWithSingleVisit):
    if len(route.route) >= 2:
        if route.route[0] == 'start' and route.route[-1] == 'end':
            return True

    return False

def day_12():
    lines = _read_file_lines('12')

    graph = {}
    net = Network() # Rendering
    for line in lines:
        parts = line.split('-')
        for part in parts:
            if part not in graph:
                graph[part] = []
                net.add_node(part)

        # Bidirectional graph with no weights
        graph[parts[0]].append(parts[1])
        graph[parts[1]].append(parts[0])
        net.add_edge(parts[0], parts[1])
        net.add_edge(parts[1], parts[0])

    # Uncomment to show in the web browser
    # net.show('12.html')

    # BFS search to find all paths, excluding invalid paths when they appear
    possible_routes = [['start']]
    found_routes = []
    while len(possible_routes) > 0:
        current_possible_routes = possible_routes.copy()
        possible_routes = []

        for route in current_possible_routes:
            new_nodes = graph[route[-1]]
            new_routes = []
            for new_node in new_nodes:
                new_route = route.copy()
                new_route.append(new_node)
                if d12_is_route_valid(new_route):
                    if d12_is_route_finished(new_route):
                        found_routes.append(new_route)
                    else:
                        possible_routes.append(new_route)

    print(len(found_routes))

    # Part 2
    possible_routes2 = [RouteWithSingleVisit(['start'], False)]
    found_routes2 = []
    while len(possible_routes2) > 0:
        current_possible_routes2 = possible_routes2.copy()
        possible_routes2 = []

        for route in current_possible_routes2:
            new_nodes = graph[route.route[-1]]
            new_routes = []
            for new_node in new_nodes:
                # Might be invalid, but that's OK! That's where validation cuts out invalid routes
                new_route1 = RouteWithSingleVisit(route.route.copy(), False)
                new_route1.route.append(new_node)
                new_route2 = RouteWithSingleVisit(route.route.copy(), True)
                new_route2.route.append(new_node)

                for new_route in [new_route1, new_route2]:
                    if d12_is_route2_valid(new_route):
                        if d12_is_route2_finished(new_route):
                            found_routes2.append(new_route)
                        else:
                            possible_routes2.append(new_route)

    print(len(found_routes2))

@dataclasses.dataclass
class Point:
    x: int
    y: int

@dataclasses.dataclass
class Instruction:
    axis_x: bool
    amount: int

def day_13():
    lines = _read_file_lines('13') # _test')
    dots = []
    instructions = []
    is_dots = True

    for line in lines:
        if line == '':
            is_dots = False
            continue

        if is_dots:
            parts = line.split(',')
            dots.append(Point(int(parts[0]), int(parts[1])))
        else:
            instruction = line.split(' ')[2].split('=')
            is_x = instruction[0] == 'x'
            instructions.append(Instruction(is_x, int(instruction[1])))

    # print(dots)
    # print(instructions)

    for instruction in instructions:
        if instruction.axis_x:
            # 'Fold' left
            for dot in dots:
                if dot.x > instruction.amount:
                    # Find difference to axis
                    difference = dot.x - instruction.amount
                    dot.x = dot.x - (2 * difference)
        else: # axis_y:
            # 'Fold' up
            for dot in dots:
                if dot.y > instruction.amount:
                    difference = dot.y - instruction.amount
                    dot.y = dot.y - (2 * difference)

        # only execute one instruction (Part 1)
        # break

    # Remove duplicate dots by putting in a dictionary
    dots_dedupped = {}
    for dot in dots:
        if dot.x not in dots_dedupped:
            dots_dedupped[dot.x] = {}

        if dot.y not in dots_dedupped[dot.x]:
            dots_dedupped[dot.x][dot.y] = 0

        dots_dedupped[dot.x][dot.y] = dots_dedupped[dot.x][dot.y] + 1

    sum = 0
    for x in dots_dedupped:
        sum = sum + len(dots_dedupped[x])

    # print(dots_dedupped)
    # print(sum)
    # Print the dots to get the answer
    x_min = min(d.x for d in dots)
    x_max = max(d.x for d in dots)
    y_min = min(d.y for d in dots)
    y_max = max(d.y for d in dots)
    for y in range(y_min, y_max + 1, 1):
        for x in range(x_min, x_max + 1, 1):
            if x in dots_dedupped and y in dots_dedupped[x]:
                print('#', end='')
            else:
                print('.', end='')

        print()


@dataclasses.dataclass
class PairRule:
    left: str
    right: str
    insert: str

def day_14():
    lines = _read_file_lines('14_test')
    template = lines[0]
    rules = []
    for line in lines[2:]:
        parts = line.split(' -> ')
        rules.append(PairRule(parts[0][0], parts[0][1], parts[1]))

    for i in range(40):
        print(i)
        new_template = ''
        for start in range(len(template) - 1):
            end = start + 1
            new_template = new_template + template[start]
            for rule in rules:
                if rule.left == template[start] and rule.right == template[end]:
                    new_template = new_template + rule.insert

        template = new_template + template[len(template) - 1]
        # print(template)

    char_distribution = {}
    for c in template:
        if c not in char_distribution:
            char_distribution[c] = 0
        char_distribution[c] = char_distribution[c] + 1

    most_frequent_char = max([char_distribution[c] for c in char_distribution])
    least_frequent_char = min([char_distribution[c] for c in char_distribution])
    print(most_frequent_char - least_frequent_char)

risk_memoizer = {}

def lowest_risk(x, y, map):
    if x not in risk_memoizer:
        risk_memoizer[x] = {}
    if x < 0 or y < 0:
        return 10000000000

    if x in risk_memoizer and y in risk_memoizer[x]:
        return risk_memoizer[x][y]

    risk = map[x][y]
    if x == 0 and y == 0:
        risk_memoizer[x][y] = risk
        return risk
    else:
        risk_memoizer[x][y] = risk + min(lowest_risk(x - 1, y, map), lowest_risk(x, y - 1, map))
        return risk + min(lowest_risk(x - 1, y, map), lowest_risk(x, y - 1, map))

def day_15():
    # Part 1
    lines = _read_file_lines('15')
    map = []
    for line in lines:
        map.append([int(x) for x in line])

    # global risk_memoizer
    # risk_memoizer = {}

    # risk = lowest_risk(0, 0, map) - map[0][0]
    # print(risk)

    # Part 2 -- the map is much bigger, wraps by 5+
    bigger_map = []
    for y in range(5 * len(map)):
        bigger_map.append([])
        for x in range(5 * len(map)):
            bigger_map[y].append(10000)

    for x in range(len(bigger_map)):
        for y in range(len(bigger_map)):
            bigger_map[y][x] = map[y % len(map)][x % len(map)]

    for x in range(len(bigger_map)):
        for y in range(len(bigger_map)):
            x_v = x
            y_v = y
            additional = 0
            while x_v >= len(map):
                additional = additional + 1
                x_v = x_v - len(map)
            while y_v >= len(map):
                additional = additional + 1
                y_v = y_v - len(map)

            bigger_map[x][y] = bigger_map[x][y] + additional
            while bigger_map[x][y] > 9:
                bigger_map[x][y] = bigger_map[x][y] - 9

    # risk_memoizer = {}

    # for line in bigger_map:
    #     for c in line:
    #         print(c, end='')
    #     print()

    risk = lowest_risk(len(bigger_map) - 1, len(bigger_map) - 1, bigger_map) - map[0][0]
    print(risk) # 2838, 2836 is too high

def d16_line_to_bits(line):
    new_line = ''
    for c in line:
        bin_value = bin(int(c, 16)).replace('0b', '')
        while len(bin_value) < 4:
            bin_value = '0' + bin_value
        new_line = new_line + bin_value
    return new_line

def day_16():
    lines = ['D2FE28']  # _read_file_lines(16)
    version_sum = 0
    for line in lines:
        bits = d16_line_to_bits(line)
        # Start parsing
        VERSION_PARSING = 1
        LITERAL_PARSING = 2
        OPERATOR_PARSING = 3
        state = VERSION_PARSING
        subpackets_left = 0
        i = 0
        while i < len(bits):
            print(state)
            if state == VERSION_PARSING:
                packet_version = bits[i:i + 3]
                packet_type = int(bits[i + 3:i + 6], 2)
                i = i + 5
                if packet_type == 4:
                    state = LITERAL_PARSING
                else:
                    state = OPERATOR_PARSING
            elif state == LITERAL_PARSING:
                # Find the first zero group
                groups = 1
                group_idx = i
                while bits[group_idx] == '1':
                    group_idx = group_idx + 5
                    groups = groups + 1

                # TODO -- parse number.
                # For now, skip entirely
                i = i + groups * 5 - 1
                print(groups)
            elif state == OPERATOR_PARSING:
                length_type = bits[i]
                if length_type == '0':
                    # Next 15 bits represent length in bits of subpackets in packet
                    pass
                else:
                    # Next 11 bits are number of subpackets in packet
                    pass
            i = i + 1

if __name__ == '__main__':
    # day_1()
    # day_2()
    # day_3()
    # day_4()
    # day_5()
    # day_6()
    # day_7()
    # day_8()
    # day_9()
    # day_10()
    # day_11()
    # day_12()
    # day_13()
    # day_14()
    # day_15()
    #day_16()
    a: int = 4.3
    b: [int] = []
    print(b)
    print(a)
    print(typ)