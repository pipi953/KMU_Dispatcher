using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    public class KMU_Astar
    {
        public List<NODE> vertices;
        public List<LINK> link_list;
        public List<TurnInfo> turninfo_list;
        public int Total_vertices;
        public float[,] adjancyMatrix;
        public float[,] adjancyMatrix_time;

        public int finded_node;


        public KMU_Astar(List<NODE> node_list, List<LINK> link_list, List<TurnInfo> turninfo_list)
        {

            Total_vertices = node_list.Count;

            this.vertices = node_list;
            this.link_list = link_list;
            this.turninfo_list = turninfo_list;

            adjancyMatrix = new float[Total_vertices, Total_vertices];

            for (int x = 0; x < Total_vertices; x++)
            {
                for (int y = 0; y < Total_vertices; y++)
                {

                    adjancyMatrix[x, y] = 0;

                }
            }

            foreach (LINK link in link_list)
            {
                int idx;
                int f_idx = vertices.FindIndex(x => x.node_id == link.f_node);
                if (f_idx == -1)
                    throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + "Matrix 생성 오류")
                    {
                        ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                    };

                int t_idx = vertices.FindIndex(x => x.node_id == link.t_node);
                if (t_idx == -1)
                    throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + "Matrix 생성 오류")
                    {
                        ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                    };

                AddEdge(f_idx, t_idx, link.length, link.max_spd);
            }

            //for (int i = 0; i < vertices.Count; i++)
            //{
            //    for (int j = 0; j < vertices.Count; j++)
            //    {
            //        if (i != j)
            //        {
            //            int idx = link_list.FindIndex(x => x.f_node == vertices[i].node_id && x.t_node == vertices[j].node_id);

            //            if (idx != -1)
            //            {
            //                AddEdge(i, j, link_list[idx].length, link_list[idx].max_spd);
            //            }

            //            //else if (result.Length > 1)
            //            //{
            //            //    throw new Exception("노드 중복 검색 에러");
            //            //}
            //        }

            //    }
            //}

        }



        void init_vertices()
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].g = 0;
                vertices[i].f = 0;
                vertices[i].Parent = null;
                vertices[i].target = null;
            }
        }
        public void AddVertex(long node_id, double X, double Y, bool turn_p)
        {
            vertices.Add(new NODE(node_id, X, Y, turn_p));
        }

        public void AddEdge(int StartVertex, int EndVertex, float Weight, int Speed)
        {
            adjancyMatrix[StartVertex, EndVertex] = (Weight / (Speed * 1000.0f)) * 3600.0f;
        }

        public NODE targetSearch(NODE vertex, List<(long, long)> tg_ft_list)
        {
            float H = float.MaxValue;
            NODE target = null;

            foreach ((long, long) tg in tg_ft_list)
            {
                NODE temp = vertices.Find(x => x.node_id == tg.Item1);
                float H_temp = vertex.H(temp);
                if (H > H_temp)
                {
                    H = H_temp;
                    target = temp;
                }
            }

            return target;
        }

        bool turnCheck(long st_node, long md_node, long ed_node, bool U_Turn_State)
        {
            bool result = true;

            //유턴유무 확인
            if (st_node == ed_node)
            {
                //Console.WriteLine("유턴");
                result = U_Turn_State;
            }

            List<TurnInfo> turninfo = turninfo_list.FindAll(x => x.st_node == st_node && x.md_node == md_node && x.ed_node == ed_node);

            if (turninfo.Count == 0 || turninfo == null)
            {
                LINK f_link = link_list.FirstOrDefault(x => x.f_node == st_node && x.t_node == md_node);
                LINK t_link = link_list.FirstOrDefault(x => x.f_node == md_node && x.t_node == ed_node);


                if (f_link != null && t_link != null)
                {
                    if (f_link.road_type != t_link.road_type)
                    {
                        if (f_link.connect_type == "000" && t_link.connect_type == "000")
                        {
                            result = false;
                        }
                    }
                }
                else
                {
                    result = false;
                }
            }

            foreach (TurnInfo info in turninfo)
            {
                switch (info.turn_type)
                {
                    case TurnInfo.TURN_TYPE.Unprotected_Turn: //비보호회전
                        result = true;
                        break;
                    case TurnInfo.TURN_TYPE.OnlyBus_Turn: //버스만회전
                        result = true;
                        break;
                    case TurnInfo.TURN_TYPE.No_Turn: //회전금지
                        result = false;
                        break;
                    case TurnInfo.TURN_TYPE.U_Turn: //U-TURN
                        result = U_Turn_State;
                        break;
                    case TurnInfo.TURN_TYPE.P_Turn: //P-TURN
                        result = true;
                        break;
                    case TurnInfo.TURN_TYPE.No_LeftTurn: //좌회전금지
                        result = false;
                        break;
                    case TurnInfo.TURN_TYPE.No_Straight: //직진금지
                        result = false;
                        break;
                    case TurnInfo.TURN_TYPE.No_RightTurn: //우회전금지
                        result = false;
                        break;
                }
            }


            return result;
        }

        (bool, (long, long)) arriveCheck(NODE current, List<(long, long)> tg_ft_list, bool U_Turn_State)
        {
            foreach ((long, long) tg in tg_ft_list)
            {
                if (tg.Item1 == current.node_id)
                {
                    long st_node = current.Parent.node_id;
                    long md_node = current.node_id;
                    long ed_node = tg.Item2;

                    int currentVertexIndex = vertices.FindIndex(x => x.node_id == current.node_id);

                    int connectCount = getConnectCount(currentVertexIndex);

                    bool temp_U_Turn_State = U_Turn_State;

                    if (connectCount == 1)
                        temp_U_Turn_State = true;


                    if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                        return (true, (tg.Item1, tg.Item2));
                }
            }

            return (false, (-1, -1));
        }

        public PathResult FindPath(long startLink, long endLink)
        {
            PathResult result;
            List<string> path_geom = new List<string>();
            List<LINK> link_list = new List<LINK>();
            double distance = 0.0d;
            List<LINK> stopoverByClosestLink = new List<LINK>();

            bool U_Turn_State = false;

            //try
            //{
            //초기화
            init_vertices();

            //변수설정
            List<(long, long)> openLinks = new List<(long, long)>();
            List<(long, long)> closedLinks = new List<(long, long)>();
            List<NODE> openVertices = new List<NODE>();


            NODE CurrentVertex;
            int Total_Vertices = vertices.Count;
            int start = -1; //시작점의 인덱스 번호
            int startParent;
            int end = -1; //타겟리스트의 인덱스 번호
            int endChild;
            LINK link;

            //기점 설정
            link = this.link_list.FirstOrDefault(x => x.link_id == startLink);
            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "시작점 설정 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };


            long start_f_node = link.f_node;
            long start_t_node = link.t_node;
            start = vertices.FindIndex(x => x.node_id == start_t_node);
            startParent = vertices.FindIndex(x => x.node_id == start_f_node);

            //종점 설정
            link = this.link_list.FirstOrDefault(x => x.link_id == endLink);
            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "끝점 설정 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

            long end_f_node = link.f_node;
            long end_t_node = link.t_node;
            end = vertices.FindIndex(x => x.node_id == end_f_node);
            endChild = vertices.FindIndex(x => x.node_id == end_t_node);

            //검색시작점 설정
            CurrentVertex = new NODE(vertices[start]);
            CurrentVertex.Parent = new NODE(vertices[startParent]);

            closedLinks.Add((start_f_node, start_t_node));
            closedLinks.Add((end_f_node, end_t_node));

            //경유지 저장변수 설정
            List<long> StopoverOrder = new List<long>();

            //stopoverByClosestLink.Add(startLink);

            while (CurrentVertex.node_id != end_t_node || CurrentVertex.Parent.node_id != end_f_node)
            {
                //연결 노드 검색
                int currentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);

                int connectCount = getConnectCount(currentVertexIndex);

                bool temp_U_Turn_State = U_Turn_State;

                if (connectCount == 1)
                    temp_U_Turn_State = true;

                for (int i = 0; i < Total_Vertices; i++)
                {
                    if (adjancyMatrix[currentVertexIndex, i] > 0)
                    {


                        //유턴유무 확인
                        long st_node = CurrentVertex.Parent.node_id;
                        long md_node = CurrentVertex.node_id;
                        long ed_node = vertices[i].node_id;

                        if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                        {
                            NODE temp = new NODE(vertices[i]);
                            if (!(closedLinks.Contains((CurrentVertex.node_id, temp.node_id))) && (!(openLinks.Contains((CurrentVertex.node_id, temp.node_id)))))
                            {
                                finded_node++;


                                openLinks.Add((CurrentVertex.node_id, temp.node_id));

                                temp.Parent = CurrentVertex;
                                openVertices.Add(temp);

                            }

                            double g;
                            double f;

                            g = CurrentVertex.g + adjancyMatrix[currentVertexIndex, i];

                            f = g + temp.H(vertices[end]);
                            temp.f = f;
                            temp.g = g;
                            temp.Parent = CurrentVertex;

                        }

                    }
                }

                closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));

                double SmallestF = double.MaxValue;
                int NextCurrent = -1;
                //Vertex NextVertex = null;

                for (int i = 0; i < openVertices.Count; i++)
                {
                    if (openVertices[i].f < SmallestF)
                    {
                        SmallestF = openVertices[i].f;
                        CurrentVertex = openVertices[i];
                        NextCurrent = i;
                    }
                }

                if (NextCurrent != -1)
                {
                    int parentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.Parent.node_id);

                    //closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                    openLinks.Remove((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                    openVertices.RemoveAt(NextCurrent);

                    if (CurrentVertex.node_id == end_f_node)
                    {
                        long st_node = CurrentVertex.Parent.node_id;
                        long md_node = CurrentVertex.node_id;
                        long ed_node = end_t_node;

                        currentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);

                        connectCount = getConnectCount(currentVertexIndex);

                        temp_U_Turn_State = U_Turn_State;

                        if (connectCount == 1)
                            temp_U_Turn_State = true;


                        if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                        {
                            NODE final_vertex = new NODE(vertices.Find(x => x.node_id == end_t_node));
                            final_vertex.Parent = CurrentVertex;
                            CurrentVertex = final_vertex;
                        }
                    }


                }
                else
                {
                    return new PathResult();
                    //throw new Exception("FindPath Error : 연결 경로 없음");
                }

            }

            //stopoverByClosestLink.Add(endLink);

            NODE v_;
            v_ = CurrentVertex;
            List<long> shortestpath = new List<long>();

            while (v_ != null)
            {
                shortestpath.Add(v_.node_id);
                v_ = v_.Parent;
            }

            shortestpath.Reverse();

            foreach (long i in shortestpath)
            {
                StopoverOrder.Add(i);
            }

            for (int i = 0; i < StopoverOrder.Count - 1; i++)
            {

                //Console.WriteLine("f :" + result[i].pathIndex[j]);
                //Console.WriteLine("t :" + result[i].pathIndex[j + 1]);

                long f_node = StopoverOrder[i];
                long t_node = StopoverOrder[i + 1];

                if (f_node != t_node)
                {
                    link = this.link_list.FirstOrDefault(x => x.f_node == f_node && x.t_node == t_node);

                    if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };





                    link_list.Add(Copy.DeepClone(link));


                    //가중치 추가
                    distance += link.length;

                }
            }

            result = new PathResult(distance, link_list, stopoverByClosestLink);

            //string path_detail = "";

            //for (int i = 0; i < path_geom.Count; i++)
            //{

            //    if (i == 0)
            //    {
            //        path_detail += path_geom[i];
            //    }
            //    else
            //    {
            //        path_detail += (", " + path_geom[i]);
            //    }
            //}

            //string text = "MULTILINESTRING ((" + path_detail + "))";


            return result;
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    return result = new PathResult();
            //}




        }

        public PathResult FindPath(long startLink, long endLink, List<(long, long)> demandList, List<LINK> lockedLink)
        {
            PathResult result;
            List<string> path_geom = new List<string>();
            List<LINK> link_list = new List<LINK>();
            double distance = 0.0d;
            List<LINK> stopoverByClosestLink = new List<LINK>();

            bool U_Turn_State = false;

            //try
            //{

            //초기화
            init_vertices();

            //변수설정
            List<(long, long)> openLinks = new List<(long, long)>();
            List<(long, long)> closedLinks = new List<(long, long)>();
            List<(long, long)> closedLinks_private = new List<(long, long)>();
            List<NODE> openVertices = new List<NODE>();

            NODE CurrentVertex;
            int Total_Vertices = vertices.Count;
            int start = -1; //시작점의 인덱스 번호
            int startParent;
            int end = -1; //타겟리스트의 인덱스 번호
            int endChild;

            LINK link;
            //기점 설정
            link = this.link_list.FirstOrDefault(x => x.link_id == startLink);
            //if (link == null) throw new Exception("FindPath Error : 시작점 설정 오류");
            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "시작점 설정 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

            long start_f_node = link.f_node;
            long start_t_node = link.t_node;
            start = vertices.FindIndex(x => x.node_id == start_t_node);
            startParent = vertices.FindIndex(x => x.node_id == start_f_node);

            //종점 설정
            link = this.link_list.FirstOrDefault(x => x.link_id == endLink);
            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "끝점 설정 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

            long end_f_node = link.f_node;
            long end_t_node = link.t_node;
            end = vertices.FindIndex(x => x.node_id == end_f_node);
            endChild = vertices.FindIndex(x => x.node_id == end_t_node);

            //검색시작점 설정
            CurrentVertex = new NODE(vertices[start]);
            CurrentVertex.Parent = new NODE(vertices[startParent]);

            closedLinks.Add((start_f_node, start_t_node));
            closedLinks_private.Add((start_f_node, start_t_node));
            closedLinks.Add((end_f_node, end_t_node));
            closedLinks_private.Add((end_f_node, end_t_node));


            //오더 설정
            //List<long> targetList = new List<long>();
            List<(long, long)> target_TF_List = new List<(long, long)>();//타겟 리스트의 인덱스 번호

            foreach ((long, long) demand in demandList)
            {
                //f_link
                link = this.link_list.FirstOrDefault(x => x.link_id == demand.Item1);
                if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "demand closest_link 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                long o_f_node = link.f_node;
                long o_t_node = link.t_node;

                closedLinks.Add((o_f_node, o_t_node));
                closedLinks_private.Add((o_f_node, o_t_node));

                //t_link
                link = this.link_list.FirstOrDefault(x => x.link_id == demand.Item2);
                if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "demand closest_link 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                long d_f_node = link.f_node;
                long d_t_node = link.t_node;

                closedLinks.Add((d_f_node, d_t_node));
                closedLinks_private.Add((d_f_node, d_t_node));

                //출발지점의 오더 확인
                if (demand.Item1 == startLink)
                    target_TF_List.Add((d_f_node, d_t_node));
                else
                    target_TF_List.Add((o_f_node, o_t_node));

            }

            target_TF_List = target_TF_List.Distinct().ToList();


            //경유지 저장변수 설정
            List<long> StopoverOrder = new List<long>();

            foreach (LINK lock_link in lockedLink)
            {
                closedLinks.Add((lock_link.f_node, lock_link.t_node));
                closedLinks_private.Add((lock_link.f_node, lock_link.t_node));
            }

            //stopoverByClosestLink.Add(startLink);

            while (target_TF_List.Count > 0)
            {
                bool arrive;
                (long, long) target_TF;

                (arrive, target_TF) = arriveCheck(CurrentVertex, target_TF_List, false);
                if (arrive)
                {

                    //도착한 지점 타겟에서 삭제
                    link = this.link_list.FirstOrDefault(x => x.f_node == target_TF.Item1 && x.t_node == target_TF.Item2);
                    if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류 (도착경유지 검색 오류) ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                    stopoverByClosestLink.Add(link);

                    long target_link_id = link.link_id;

                    target_TF_List.RemoveAll(x => x.Item1 == target_TF.Item1 && x.Item2 == target_TF.Item2);

                    List<(long, long)> complete_orderList = new List<(long, long)>();

                    //승차지점일시 하차지점 추가
                    foreach ((long, long) demand in demandList)
                    {
                        if (demand.Item1 == target_link_id)
                        {
                            complete_orderList.Add(demand);

                            link = this.link_list.FirstOrDefault(x => x.link_id == demand.Item2);
                            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류 (승차지점 하차지점경유지 설정 오류) ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };



                            long target_f_node = link.f_node;
                            long target_t_node = link.t_node;

                            target_TF_List.Add((target_f_node, target_t_node));
                        }
                    }

                    foreach ((long, long) comp in complete_orderList)
                    {
                        demandList.RemoveAll(x => x.Item1 == comp.Item1 && x.Item2 == comp.Item2);
                    }


                    target_TF_List = target_TF_List.Distinct().ToList();

                    //변수 재설정



                    start = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);
                    openLinks.Clear();
                    closedLinks.Clear();
                    openVertices.Clear();

                    closedLinks.AddRange(closedLinks_private);


                    //도착지까지의 경로 저장
                    NODE v;
                    v = CurrentVertex;

                    List<long> shortestpath = new List<long>();

                    shortestpath.Add(v.node_id);

                    while (v.Parent != null)
                    {
                        shortestpath.Add(v.Parent.node_id);
                        closedLinks.Add((v.Parent.node_id, v.node_id));
                        //Console.WriteLine(v.Index + ",");
                        v = v.Parent;

                    }
                    shortestpath.Reverse();

                    foreach (long i in shortestpath)
                    {
                        StopoverOrder.Add(i);
                    }

                    //closedLinks = closedLinks.Distinct().ToList();

                    NODE final_vertex = new NODE(vertices.Find(x => x.node_id == target_TF.Item2));
                    final_vertex.Parent = CurrentVertex;
                    CurrentVertex = final_vertex;

                    CurrentVertex.Parent.Parent = null;
                    CurrentVertex.f = 0;
                    CurrentVertex.g = 0;

                }
                else
                {
                    //연결노드 검색
                    int currentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);

                    int connectCount = getConnectCount(currentVertexIndex);

                    bool temp_U_Turn_State = U_Turn_State;

                    if (connectCount == 1)
                        temp_U_Turn_State = true;


                    for (int i = 0; i < Total_Vertices; i++)
                    {
                        if (adjancyMatrix[currentVertexIndex, i] > 0)
                        {
                            //유턴유무 확인
                            long st_node = CurrentVertex.Parent.node_id;
                            long md_node = CurrentVertex.node_id;
                            long ed_node = vertices[i].node_id;

                            

                            if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                            {
                                NODE temp = new NODE(vertices[i]);
                                if ((!(closedLinks.Contains((CurrentVertex.node_id, temp.node_id)))) && (!(openLinks.Contains((CurrentVertex.node_id, temp.node_id)))))
                                {
                                    finded_node++;


                                    openLinks.Add((CurrentVertex.node_id, temp.node_id));

                                    temp.Parent = CurrentVertex;
                                    openVertices.Add(temp);

                                    double g;
                                    double f;

                                    temp.target = targetSearch(temp, target_TF_List);
                                    g = CurrentVertex.g + adjancyMatrix[currentVertexIndex, i];


                                    f = g + temp.H(temp.target);
                                    temp.f = f;
                                    temp.g = g;
                                    temp.Parent = CurrentVertex;
                                }


                            }
                        }
                    }

                    closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));


                    double SmallestF = double.MaxValue;
                    int NextCurrent = -1;
                    //Vertex NextVertex = null;

                    for (int i = 0; i < openVertices.Count; i++)
                    {
                        if (openVertices[i].f < SmallestF)
                        {
                            SmallestF = openVertices[i].f;
                            CurrentVertex = openVertices[i];
                            NextCurrent = i;
                        }
                    }

                    if (NextCurrent != -1)
                    {
                        int parentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.Parent.node_id);

                        //closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                        openLinks.Remove((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                        //openLinks.RemoveAll(x=>x.Item1 == CurrentVertex.Parent.node_id && x.Item2 == CurrentVertex.node_id);
                        openVertices.RemoveAt(NextCurrent);

                    }
                    else
                    {
                        //throw new Exception("FindPath Error : 연결 경로 없음");
                        return new PathResult();
                    }

                }

            }



            //종점 경로 찾기
            while (CurrentVertex.node_id != end_t_node || CurrentVertex.Parent.node_id != end_f_node)
            {
                //연결 노드 검색
                int currentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);
                int connectCount = getConnectCount(currentVertexIndex);

                bool temp_U_Turn_State = U_Turn_State;

                if (connectCount == 1)
                    temp_U_Turn_State = true;


                for (int i = 0; i < Total_Vertices; i++)
                {
                    if (adjancyMatrix[currentVertexIndex, i] > 0)
                    {

                        //유턴유무 확인
                        long st_node = CurrentVertex.Parent.node_id;
                        long md_node = CurrentVertex.node_id;
                        long ed_node = vertices[i].node_id;

                        if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                        {
                            NODE temp = new NODE(vertices[i]);
                            if (!(closedLinks.Contains((CurrentVertex.node_id, temp.node_id))) && (!(openLinks.Contains((CurrentVertex.node_id, temp.node_id)))))
                            {
                                finded_node++;


                                openLinks.Add((CurrentVertex.node_id, temp.node_id));

                                temp.Parent = CurrentVertex;
                                openVertices.Add(temp);

                            }

                            double g;
                            double f;

                            g = CurrentVertex.g + adjancyMatrix[currentVertexIndex, i];

                            f = g + temp.H(vertices[end]);
                            temp.f = f;
                            temp.g = g;
                            temp.Parent = CurrentVertex;

                        }

                    }
                }


                closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));

                double SmallestF = double.MaxValue;
                int NextCurrent = -1;
                //Vertex NextVertex = null;

                for (int i = 0; i < openVertices.Count; i++)
                {
                    if (openVertices[i].f < SmallestF)
                    {
                        SmallestF = openVertices[i].f;
                        CurrentVertex = openVertices[i];
                        NextCurrent = i;
                    }
                }

                if (NextCurrent != -1)
                {
                    int parentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.Parent.node_id);

                    //closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                    openLinks.Remove((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                    openVertices.RemoveAt(NextCurrent);

                    if (CurrentVertex.node_id == end_f_node)
                    {
                        long st_node = CurrentVertex.Parent.node_id;
                        long md_node = CurrentVertex.node_id;
                        long ed_node = end_t_node;

                        currentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);

                        connectCount = getConnectCount(currentVertexIndex);

                        temp_U_Turn_State = U_Turn_State;

                        if (connectCount == 1)
                            temp_U_Turn_State = true;


                        if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                        {
                            NODE final_vertex = new NODE(vertices.Find(x => x.node_id == end_t_node));
                            final_vertex.Parent = CurrentVertex;
                            CurrentVertex = final_vertex;
                        }
                    }
                }
                else
                {
                    //throw new Exception("FindPath Error : 연결 경로 없음");
                    return result = new PathResult();
                }

            }


            //stopoverByClosestLink.Add(endLink);

            NODE v_;
            v_ = CurrentVertex;
            List<long> _shortestpath = new List<long>();

            while (v_ != null)
            {
                _shortestpath.Add(v_.node_id);
                v_ = v_.Parent;
            }

            _shortestpath.Reverse();

            foreach (long i in _shortestpath)
            {
                StopoverOrder.Add(i);
            }


            for (int i = 0; i < StopoverOrder.Count - 1; i++)
            {

                //Console.WriteLine("f :" + result[i].pathIndex[j]);
                //Console.WriteLine("t :" + result[i].pathIndex[j + 1]);

                long f_node = StopoverOrder[i];
                long t_node = StopoverOrder[i + 1];

                if (f_node != t_node)
                {
                    link = this.link_list.FirstOrDefault(x => x.f_node == f_node && x.t_node == t_node);
                    if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류 (도착경유지 검색 오류) ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                    link_list.Add(Copy.DeepClone(link));


                    //가중치 추가
                    distance += link.length;

                }
            }

            result = new PathResult(distance, link_list, stopoverByClosestLink);

            //var duplicates = link_list.GroupBy(x => x.link_id).Where(g => g.Count() > 1).Select(g => g.Key);

            //if(duplicates.Count() > 0)
            //{
            //    Console.Write("중복링크 : ");
            //    foreach (var item in duplicates)
            //    {
            //        Console.Write("'" + item + "',");
            //    }
            //    Console.WriteLine();
            //}


            return result;
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    return result = new PathResult();
            //}



        }

        public PathResult FindPath(long startLink, List<(long, long)> demandList, List<LINK> lockedLink)
        {
            PathResult result;
            List<string> path_geom = new List<string>();
            List<LINK> link_list = new List<LINK>();
            double distance = 0.0d;
            List<LINK> stopoverByClosestLink = new List<LINK>();

            bool U_Turn_State = false;

            //try
            //{

            //초기화
            init_vertices();

            //변수설정
            List<(long, long)> saveOpenLinks = new List<(long, long)>();
            List<(long, long)> openLinks = new List<(long, long)>();
            List<(long, long)> closedLinks = new List<(long, long)>();
            List<(long, long)> closedLinks_private = new List<(long, long)>();
            List<NODE> openVertices = new List<NODE>();

            NODE CurrentVertex;
            int Total_Vertices = vertices.Count;
            int start = -1; //시작점의 인덱스 번호
            int startParent;
            int end = -1; //타겟리스트의 인덱스 번호
            int endChild;

            LINK link;
            //기점 설정
            link = this.link_list.FirstOrDefault(x => x.link_id == startLink);
            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "시작점 설정 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

            long start_f_node = link.f_node;
            long start_t_node = link.t_node;
            start = vertices.FindIndex(x => x.node_id == start_t_node);
            startParent = vertices.FindIndex(x => x.node_id == start_f_node);


            //검색시작점 설정
            CurrentVertex = new NODE(vertices[start]);
            CurrentVertex.Parent = new NODE(vertices[startParent]);

            closedLinks.Add((start_f_node, start_t_node));
            closedLinks_private.Add((start_f_node, start_t_node));


            //오더 설정
            //List<long> targetList = new List<long>();
            List<(long, long)> target_TF_List = new List<(long, long)>();//타겟 리스트의 인덱스 번호

            foreach ((long, long) demand in demandList)
            {
                //f_link
                link = this.link_list.FirstOrDefault(x => x.link_id == demand.Item1);
                if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "demand closest_link 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                long o_f_node = link.f_node;
                long o_t_node = link.t_node;

                closedLinks.Add((o_f_node, o_t_node));
                closedLinks_private.Add((o_f_node, o_t_node));

                //t_link
                link = this.link_list.FirstOrDefault(x => x.link_id == demand.Item2);
                if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "demand closest_link 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                long d_f_node = link.f_node;
                long d_t_node = link.t_node;

                closedLinks.Add((d_f_node, d_t_node));
                closedLinks_private.Add((d_f_node, d_t_node));

                //출발지점의 오더 확인
                if (demand.Item1 == startLink)
                {
                    target_TF_List.Add((d_f_node, d_t_node));

                    link = this.link_list.FirstOrDefault(x => x.link_id == startLink);
                    if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "시작점 설정 오류") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                    stopoverByClosestLink.Add(link);
                } 
                else
                    target_TF_List.Add((o_f_node, o_t_node));

            }

            target_TF_List = target_TF_List.Distinct().ToList();


            //경유지 저장변수 설정
            List<long> StopoverOrder = new List<long>();

            foreach (LINK lock_link in lockedLink)
            {
                closedLinks.Add((lock_link.f_node, lock_link.t_node));
                closedLinks_private.Add((lock_link.f_node, lock_link.t_node));
            }

            //stopoverByClosestLink.Add(startLink);

            while (target_TF_List.Count > 0)
            {
                bool arrive;
                (long, long) target_TF;

                (arrive, target_TF) = arriveCheck(CurrentVertex, target_TF_List, false);
                if (arrive)
                {

                    //도착한 지점 타겟에서 삭제
                    link = this.link_list.FirstOrDefault(x => x.f_node == target_TF.Item1 && x.t_node == target_TF.Item2);
                    if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류 (도착경유지 검색 오류) ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                    stopoverByClosestLink.Add(link);

                    long target_link_id = link.link_id;

                    target_TF_List.RemoveAll(x => x.Item1 == target_TF.Item1 && x.Item2 == target_TF.Item2);

                    List<(long, long)> complete_orderList = new List<(long, long)>();

                    //승차지점일시 하차지점 추가
                    foreach ((long, long) demand in demandList)
                    {
                        if (demand.Item1 == target_link_id)
                        {
                            complete_orderList.Add(demand);

                            link = this.link_list.FirstOrDefault(x => x.link_id == demand.Item2);
                            if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류 (승차지점 하차지점경유지 설정 오류) ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };



                            long target_f_node = link.f_node;
                            long target_t_node = link.t_node;

                            target_TF_List.Add((target_f_node, target_t_node));
                        }
                    }

                    foreach ((long, long) comp in complete_orderList)
                    {
                        demandList.RemoveAll(x => x.Item1 == comp.Item1 && x.Item2 == comp.Item2);
                    }


                    target_TF_List = target_TF_List.Distinct().ToList();

                    //변수 재설정



                    start = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);
                    openLinks.Clear();
                    closedLinks.Clear();
                    openVertices.Clear();

                    closedLinks.AddRange(closedLinks_private);


                    //도착지까지의 경로 저장
                    NODE v;
                    v = CurrentVertex;

                    List<long> shortestpath = new List<long>();
                    
                    shortestpath.Add(v.node_id);

                    while (v.Parent != null)
                    {
                        shortestpath.Add(v.Parent.node_id);
                        //closedLinks.Add((v.Parent.node_id, v.node_id));
                        //Console.WriteLine(v.Index + ",");
                        v = v.Parent;

                    }
                    shortestpath.Reverse();

                    foreach (long i in shortestpath)
                    {
                        StopoverOrder.Add(i);
                    }

                    for(int i =0; i< StopoverOrder.Count-1; i++)
                    {
                        closedLinks.Add((StopoverOrder[i], StopoverOrder[i+1]));
                    }
                    

                    //closedLinks = closedLinks.Distinct().ToList();

                    NODE final_vertex = new NODE(vertices.Find(x => x.node_id == target_TF.Item2));
                    final_vertex.Parent = CurrentVertex;
                    CurrentVertex = final_vertex;

                    CurrentVertex.Parent.Parent = null;
                    CurrentVertex.f = 0;
                    CurrentVertex.g = 0;

                    if (target_TF_List.Count == 0)
                        StopoverOrder.Add(target_TF.Item2);

                }
                else
                {
                    //연결노드 검색
                    int currentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.node_id);

                    int connectCount = getConnectCount(currentVertexIndex);

                    bool temp_U_Turn_State = U_Turn_State;

                    if (connectCount == 1)
                        temp_U_Turn_State = true;


                    for (int i = 0; i < Total_Vertices; i++)
                    {
                        if (adjancyMatrix[currentVertexIndex, i] > 0)
                        {

                            //유턴유무 확인
                            long st_node = CurrentVertex.Parent.node_id;
                            long md_node = CurrentVertex.node_id;
                            long ed_node = vertices[i].node_id;

                            if (turnCheck(st_node, md_node, ed_node, temp_U_Turn_State))
                            {
                                NODE temp = new NODE(vertices[i]);
                                if ((!(closedLinks.Contains((CurrentVertex.node_id, temp.node_id)))) && (!(openLinks.Contains((CurrentVertex.node_id, temp.node_id)))))
                                {
                                    finded_node++;


                                    openLinks.Add((CurrentVertex.node_id, temp.node_id));
                                    //saveOpenLinks.Add((CurrentVertex.node_id, temp.node_id));

                                    temp.Parent = CurrentVertex;
                                    openVertices.Add(temp);

                                    double g;
                                    double f;

                                    temp.target = targetSearch(temp, target_TF_List);
                                    g = CurrentVertex.g + adjancyMatrix[currentVertexIndex, i];


                                    f = g + temp.H(temp.target);
                                    temp.f = f;
                                    temp.g = g;
                                    temp.Parent = CurrentVertex;
                                }


                            }
                        }
                    }

                    closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));


                    double SmallestF = double.MaxValue;
                    int NextCurrent = -1;
                    //Vertex NextVertex = null;

                    for (int i = 0; i < openVertices.Count; i++)
                    {
                        if (openVertices[i].f < SmallestF)
                        {
                            SmallestF = openVertices[i].f;
                            CurrentVertex = openVertices[i];
                            NextCurrent = i;
                        }
                    }

                    if (NextCurrent != -1)
                    {
                        int parentVertexIndex = vertices.FindIndex(x => x.node_id == CurrentVertex.Parent.node_id);

                        //closedLinks.Add((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                        openLinks.Remove((CurrentVertex.Parent.node_id, CurrentVertex.node_id));
                        //openLinks.RemoveAll(x=>x.Item1 == CurrentVertex.Parent.node_id && x.Item2 == CurrentVertex.node_id);
                        openVertices.RemoveAt(NextCurrent);

                    }
                    else
                    {
                        //throw new Exception("FindPath Error : 연결 경로 없음");
                        return new PathResult();
                    }

                }

            }

            for (int i = 0; i < StopoverOrder.Count - 1; i++)
            {

                //Console.WriteLine("f :" + result[i].pathIndex[j]);
                //Console.WriteLine("t :" + result[i].pathIndex[j + 1]);

                long f_node = StopoverOrder[i];
                long t_node = StopoverOrder[i + 1];

                if (f_node != t_node)
                {
                    link = this.link_list.FirstOrDefault(x => x.f_node == f_node && x.t_node == t_node);
                    if (link == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.FindPathError + "] - " + "계산 오류 (도착경유지 검색 오류) ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };

                    //int findIdx = link_list.FindIndex(x => x.link_id == link.link_id);

                    //if (findIdx != -1)
                    //    return new PathResult();

                    link_list.Add(Copy.DeepClone(link));


                    //가중치 추가
                    distance += link.length;

                }
            }

            result = new PathResult(distance, link_list, stopoverByClosestLink);

            //var duplicates = link_list.GroupBy(x => x.link_id).Where(g => g.Count() > 1).Select(g => g.Key);

            //if(duplicates.Count() > 0)
            //{
            //    Console.Write("중복링크 : ");
            //    foreach (var item in duplicates)
            //    {
            //        Console.Write("'" + item + "',");
            //    }
            //    Console.WriteLine();
            //}

            //result.saveOpenLinks = saveOpenLinks;

            return result;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //    return result = new PathResult();
            //}
        }

        public int getConnectCount(int currentVertexIndex)
        {
            int result = 0;
            int Total_Vertices = vertices.Count;

            for (int i = 0; i < Total_Vertices; i++)
            {
                if (adjancyMatrix[currentVertexIndex, i] > 0)
                    result++;
            }

            return result;
        }
    }
}
